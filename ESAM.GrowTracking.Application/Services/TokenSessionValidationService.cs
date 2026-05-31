using ESAM.GrowTracking.Application.Abstractions.Services;
using ESAM.GrowTracking.Application.Abstractions.Services.Results;
using ESAM.GrowTracking.Domain.Abstractions.DataAccess.Repositories;
using ESAM.GrowTracking.Domain.Entities;
using ESAM.GrowTracking.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace ESAM.GrowTracking.Application.Services
{
    public class TokenSessionValidationService : ITokenSessionValidationService
    {
        private readonly ILogger<TokenSessionValidationService> _logger;
        private readonly IHashService _hashService;
        private readonly IUserRepository _userRepository;
        private readonly IUserDeviceRepository _userDeviceRepository;
        private readonly IUserSessionWorkProfileSelectedRepository _userSessionWorkProfileSelectedRepository;
        private readonly IUserWorkProfileRepository _userWorkProfileRepository;
        private readonly IWorkProfileRepository _workProfileRepository;
        private readonly IUserSessionRoleCampusSelectedRepository _userSessionRoleCampusSelectedRepository;
        private readonly IWorkProfilePermissionRepository _workProfilePermissionRepository;
        private readonly IUserRoleCampusRepository _userRoleCampusRepository;
        private readonly IRolePermissionRepository _rolePermissionRepository;

        public TokenSessionValidationService(ILogger<TokenSessionValidationService> logger, IHashService hashService, IUserRepository userRepository,
            IUserDeviceRepository userDeviceRepository, IUserSessionWorkProfileSelectedRepository userSessionWorkProfileSelectedRepository,
            IUserWorkProfileRepository userWorkProfileRepository, IWorkProfileRepository workProfileRepository,
            IUserSessionRoleCampusSelectedRepository userSessionRoleCampusSelectedRepository, IWorkProfilePermissionRepository workProfilePermissionRepository,
            IUserRoleCampusRepository userRoleCampusRepository, IRolePermissionRepository rolePermissionRepository)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(hashService);
            ArgumentNullException.ThrowIfNull(userRepository);
            ArgumentNullException.ThrowIfNull(userDeviceRepository);
            ArgumentNullException.ThrowIfNull(userSessionWorkProfileSelectedRepository);
            ArgumentNullException.ThrowIfNull(userWorkProfileRepository);
            ArgumentNullException.ThrowIfNull(workProfileRepository);
            ArgumentNullException.ThrowIfNull(userSessionRoleCampusSelectedRepository);
            ArgumentNullException.ThrowIfNull(workProfilePermissionRepository);
            ArgumentNullException.ThrowIfNull(userRoleCampusRepository);
            ArgumentNullException.ThrowIfNull(rolePermissionRepository);
            _logger = logger;
            _hashService = hashService;
            _userRepository = userRepository;
            _userDeviceRepository = userDeviceRepository;
            _userSessionWorkProfileSelectedRepository = userSessionWorkProfileSelectedRepository;
            _userWorkProfileRepository = userWorkProfileRepository;
            _workProfileRepository = workProfileRepository;
            _userSessionRoleCampusSelectedRepository = userSessionRoleCampusSelectedRepository;
            _workProfilePermissionRepository = workProfilePermissionRepository;
            _userRoleCampusRepository = userRoleCampusRepository;
            _rolePermissionRepository = rolePermissionRepository;
        }

        public async Task<TokenSessionValidationResult> ValidateAsync(UserSession userSession, UserSessionRefreshToken userSessionRefreshToken, string? refreshTokenPlain,
            string? deviceIdentifier, string revokedReasonPrefix, bool isAuthenticated, int currentUserId, int? currentWorkProfileId, int? currentRoleId, int? currentCampusId,
            int? currentTokenVersion, string? currentSecurityStamp, DateTime utcNow, bool asTracking = false, CancellationToken cancellationToken = default)
        {
            if (userSession.AbsoluteExpiresAt <= utcNow)
                return TokenSessionValidationResult.Failure(revokedReasonPrefix + " Sesión absoluta expirada.");
            if (userSessionRefreshToken.ExpiresAt <= utcNow || userSession.ExpiresAt <= utcNow)
                return TokenSessionValidationResult.Failure(revokedReasonPrefix + " Refresh token o sesión expirada.");
            if (userSessionRefreshToken.IsRevoked || userSession.IsRevoked)
                return TokenSessionValidationResult.Failure(revokedReasonPrefix + " Refresh token o sesión ya revocada.");
            if (string.IsNullOrWhiteSpace(refreshTokenPlain) || !_hashService.VerifyHash(refreshTokenPlain, userSessionRefreshToken.Salt, userSessionRefreshToken.TokenHash))
            {
                _logger.LogWarning("TokenSessionValidationService: hash del refresh token no coincide o token no provisto. UserSessionId={UserSessionId}, Identificador={Identifier}",
                    userSession.Id, userSessionRefreshToken.Identifier);
                return TokenSessionValidationResult.Failure(revokedReasonPrefix + " Refresh token inválido.");
            }
            if (userSessionRefreshToken.ReplacedByUserSessionRefreshTokenId.HasValue)
                return TokenSessionValidationResult.Failure(revokedReasonPrefix + " Refresh token reemplazado.");
            var user = await _userRepository.GetByIdAsync(userSession.UserId, asTracking, cancellationToken);
            if (user is null || user.IsDeleted || user.IsLocked(utcNow))
            {
                _logger.LogWarning("TokenSessionValidationService: usuario no encontrado, deshabilitado o bloqueado. UserSessionId={UserSessionId}, UserId={UserId}", 
                    userSession.Id,userSession.UserId);
                return TokenSessionValidationResult.Failure(revokedReasonPrefix + " Usuario no encontrado, deshabilitado o bloqueado.");
            }
            var userDevice = await _userDeviceRepository.GetByIdAndUserIdAsync(userSession.UserDeviceId, userSession.UserId, asTracking, cancellationToken);
            if (userDevice is null || userDevice.IsDeleted || userDevice.IsLocked(utcNow))
            {
                _logger.LogWarning("TokenSessionValidationService: dispositivo no encontrado, deshabilitado o bloqueado. UserSessionId={UserSessionId}, " + 
                    "UserDeviceId={UserDeviceId}", userSession.Id, userSession.UserDeviceId);
                return TokenSessionValidationResult.Failure(revokedReasonPrefix + " Dispositivo no encontrado, deshabilitado o bloqueado.");
            }
            if (!string.Equals(deviceIdentifier, userDevice.DeviceIdentifier, StringComparison.Ordinal))
            {
                _logger.LogWarning("TokenSessionValidationService: DeviceIdentifier no coincide. Esperado={Expected}, Provisto={Provided}, UserSessionId={UserSessionId}",
                    userDevice.DeviceIdentifier, deviceIdentifier, userSession.Id);
                return TokenSessionValidationResult.Failure(revokedReasonPrefix + " DeviceIdentifier no coincide.");
            }
            var userSessionWorkProfileSelected = await _userSessionWorkProfileSelectedRepository.GetByUserSessionIdAsync(userSession.Id, asTracking, cancellationToken);
            if (userSessionWorkProfileSelected is null)
            {
                _logger.LogWarning("TokenSessionValidationService: la sesión no tiene perfil de trabajo seleccionado. UserSessionId={UserSessionId}", userSession.Id);
                return TokenSessionValidationResult.Failure(revokedReasonPrefix + " La sesión no tiene perfil de trabajo seleccionado.");
            }
            var userWorkProfile = await _userWorkProfileRepository.GetByUserIdAndWorkProfileIdAsync(userSessionWorkProfileSelected.UserId, 
                userSessionWorkProfileSelected.WorkProfileId, asTracking, cancellationToken);
            if (userWorkProfile is null || userWorkProfile.IsDeleted)
            {
                _logger.LogWarning("TokenSessionValidationService: perfil de trabajo no activo o no asignado. UserSessionId={UserSessionId}, WorkProfileId={WorkProfileId}",
                    userSession.Id,userSessionWorkProfileSelected.WorkProfileId);
                return TokenSessionValidationResult.Failure(revokedReasonPrefix + " Perfil de trabajo no activo o no asignado.");
            }
            var workProfileType = await _workProfileRepository.GetWorkProfileTypeByIdAsync(userWorkProfile.WorkProfileId, asTracking, cancellationToken);
            if (workProfileType != WorkProfileType.WithRoles && workProfileType != WorkProfileType.OnlyWorkProfile)
            {
                _logger.LogWarning("TokenSessionValidationService: tipo de perfil de trabajo no válido. WorkProfileId={WorkProfileId}, Tipo={Type}", userWorkProfile.WorkProfileId, 
                    workProfileType);
                return TokenSessionValidationResult.Failure(revokedReasonPrefix + " Tipo de perfil de trabajo no válido.");
            }
            var userSessionRoleCampusSelected = await _userSessionRoleCampusSelectedRepository.GetByUserSessionIdAsync(userSession.Id, asTracking, cancellationToken);
            if (workProfileType == WorkProfileType.OnlyWorkProfile)
            {
                if (userSessionRoleCampusSelected is not null)
                {
                    _logger.LogWarning("TokenSessionValidationService: tipo OnlyWorkProfile no debe tener rol de sede seleccionado. UserSessionId={UserSessionId}", userSession.Id);
                    return TokenSessionValidationResult.Failure(revokedReasonPrefix + " Tipo OnlyWorkProfile No debe tener rol de sede seleccionado.");
                }
                var hasActiveWorkProfilePermissions = await _workProfilePermissionRepository.HasActivePermissionsAsync(userWorkProfile.WorkProfileId, asTracking, cancellationToken);
                if (!hasActiveWorkProfilePermissions)
                {
                    _logger.LogWarning("TokenSessionValidationService: perfil de trabajo sin permisos activos. WorkProfileId={WorkProfileId}", userWorkProfile.WorkProfileId);
                    return TokenSessionValidationResult.Failure(revokedReasonPrefix + " Perfil de trabajo no tiene permisos asignados.");
                }
                if (isAuthenticated)
                {
                    if (user.SecurityStamp != currentSecurityStamp || user.TokenVersion != currentTokenVersion)
                    {
                        _logger.LogWarning("TokenSessionValidationService: discrepancia de contexto de seguridad en OnlyWorkProfile. UserId={UserId}", user.Id);
                        return TokenSessionValidationResult.Failure(revokedReasonPrefix + " SecurityStamp o TokenVersion no coincidentes con el contexto actual.");
                    }
                    if (userSessionWorkProfileSelected.WorkProfileId != currentWorkProfileId)
                    {
                        _logger.LogWarning("TokenSessionValidationService: perfil de trabajo seleccionado no coincide con el contexto autenticado OnlyWorkProfile. " + 
                            "UserSessionId={UserSessionId}", userSession.Id);
                        return TokenSessionValidationResult.Failure(revokedReasonPrefix + " Perfil seleccionado en la sesión no coincide con el contexto autenticado.");
                    }
                }
                return TokenSessionValidationResult.Success(revokedReasonPrefix + " Correcto.", user, userSessionWorkProfileSelected.WorkProfileId, null, null);
            }
            if (userSessionRoleCampusSelected is null)
            {
                _logger.LogWarning("TokenSessionValidationService: sesión de tipo WithRoles sin rol de sede seleccionado. UserSessionId={UserSessionId}", userSession.Id);
                return TokenSessionValidationResult.Failure(revokedReasonPrefix + " Tipo WithRoles requiere rol de sede seleccionado.");
            }
            var userRoleCampus = await _userRoleCampusRepository.GetByUserIdAndRoleIdAndCampusIdAsync(userSessionRoleCampusSelected.UserId, userSessionRoleCampusSelected.RoleId,
                userSessionRoleCampusSelected.CampusId, asTracking, cancellationToken);
            if (userRoleCampus is null || userRoleCampus.IsDeleted)
            {
                _logger.LogWarning("TokenSessionValidationService: rol de sede no activo o no asignado. UserSessionId={UserSessionId}, RoleId={RoleId}, CampusId={CampusId}",
                    userSession.Id, userSessionRoleCampusSelected.RoleId, userSessionRoleCampusSelected.CampusId);
                return TokenSessionValidationResult.Failure(revokedReasonPrefix + " Rol de sede no activo o no asignado.");
            }
            var hasActiveRolePermissions = await _rolePermissionRepository.HasActivePermissionsAsync(userRoleCampus.RoleId, asTracking, cancellationToken);
            if (!hasActiveRolePermissions)
            {
                _logger.LogWarning("TokenSessionValidationService: rol sin permisos activos. RoleId={RoleId}", userRoleCampus.RoleId);
                return TokenSessionValidationResult.Failure(revokedReasonPrefix + " Rol no tiene permisos asignados.");
            }
            if (isAuthenticated)
            {
                if (user.SecurityStamp != currentSecurityStamp || user.TokenVersion != currentTokenVersion)
                {
                    _logger.LogWarning("TokenSessionValidationService: discrepancia de contexto de seguridad en WithRoles. UserId={UserId}", user.Id);
                    return TokenSessionValidationResult.Failure(revokedReasonPrefix + " SecurityStamp o TokenVersion no coincidentes con el contexto actual.");
                }
                if (userSessionWorkProfileSelected.WorkProfileId != currentWorkProfileId)
                {
                    _logger.LogWarning("TokenSessionValidationService: perfil de trabajo seleccionado no coincide con el contexto autenticado WithRoles. " + 
                        "UserSessionId={UserSessionId}", userSession.Id);
                    return TokenSessionValidationResult.Failure(revokedReasonPrefix + " Perfil seleccionado en la sesión no coincide con el contexto autenticado.");
                }
                if (userSessionRoleCampusSelected.RoleId != currentRoleId || userSessionRoleCampusSelected.CampusId != currentCampusId)
                {
                    _logger.LogWarning("TokenSessionValidationService: rol de sede no coincide con el contexto autenticado. UserSessionId={UserSessionId}", userSession.Id);
                    return TokenSessionValidationResult.Failure(revokedReasonPrefix + " Rol de sede seleccionado en la sesión no coincide con el contexto autenticado.");
                }
            }
            return TokenSessionValidationResult.Success(revokedReasonPrefix + " Correcto.", user, userSessionWorkProfileSelected.WorkProfileId,
                userSessionRoleCampusSelected.RoleId, userSessionRoleCampusSelected.CampusId);
        }
    }
}