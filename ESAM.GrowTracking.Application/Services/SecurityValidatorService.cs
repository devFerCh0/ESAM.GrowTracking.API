using ESAM.GrowTracking.Application.Abstractions.Services;
using ESAM.GrowTracking.Application.Results;
using ESAM.GrowTracking.Application.ValueObjects;
using ESAM.GrowTracking.Domain.Abstractions.DataAccess.Repositories;
using ESAM.GrowTracking.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace ESAM.GrowTracking.Application.Services
{
    public sealed class SecurityValidatorService : ISecurityValidatorService
    {
        private readonly ILogger<SecurityValidatorService> _logger;
        private readonly IBlacklistedAccessTokenTemporaryRepository _blacklistedAccessTokenTemporaryRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUserDeviceRepository _userDeviceRepository;
        private readonly IBlacklistedAccessTokenSessionRepository _blacklistedAccessTokenSessionRepository;
        private readonly IUserWorkProfileRepository _userWorkProfileRepository;
        private readonly IWorkProfilePermissionRepository _workProfilePermissionRepository;
        private readonly IUserRoleCampusRepository _userRoleCampusRepository;
        private readonly IRolePermissionRepository _rolePermissionRepository;
        private readonly IUserSessionRepository _userSessionRepository;
        private readonly IDateTimeService _dateTimeService;

        public SecurityValidatorService(ILogger<SecurityValidatorService> logger, IBlacklistedAccessTokenTemporaryRepository blacklistedAccessTokenTemporaryRepository, 
            IUserRepository userRepository, IUserDeviceRepository userDeviceRepository, IBlacklistedAccessTokenSessionRepository blacklistedAccessTokenSessionRepository, 
            IUserWorkProfileRepository userWorkProfileRepository, IWorkProfilePermissionRepository workProfilePermissionRepository, 
            IUserRoleCampusRepository userRoleCampusRepository, IRolePermissionRepository rolePermissionRepository, IUserSessionRepository userSessionRepository, 
            IDateTimeService dateTimeService)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(blacklistedAccessTokenTemporaryRepository);
            ArgumentNullException.ThrowIfNull(userRepository);
            ArgumentNullException.ThrowIfNull(userDeviceRepository);
            ArgumentNullException.ThrowIfNull(blacklistedAccessTokenSessionRepository);
            ArgumentNullException.ThrowIfNull(userWorkProfileRepository);
            ArgumentNullException.ThrowIfNull(workProfilePermissionRepository);
            ArgumentNullException.ThrowIfNull(userRoleCampusRepository);
            ArgumentNullException.ThrowIfNull(rolePermissionRepository);
            ArgumentNullException.ThrowIfNull(userSessionRepository);
            ArgumentNullException.ThrowIfNull(dateTimeService);
            _logger = logger;
            _blacklistedAccessTokenTemporaryRepository = blacklistedAccessTokenTemporaryRepository;
            _userRepository = userRepository;
            _userDeviceRepository = userDeviceRepository;
            _blacklistedAccessTokenSessionRepository = blacklistedAccessTokenSessionRepository;
            _userWorkProfileRepository = userWorkProfileRepository;
            _workProfilePermissionRepository = workProfilePermissionRepository;
            _userRoleCampusRepository = userRoleCampusRepository;
            _rolePermissionRepository = rolePermissionRepository;
            _userSessionRepository = userSessionRepository;
            _dateTimeService = dateTimeService;
        }

        public async Task<Result> ValidateBlacklistedAccessTokenTemporaryAsync(string jti, bool asTracking, CancellationToken cancellationToken)
        {
            var doesBlacklistedAccessTokenTemporaryNotExist = await _blacklistedAccessTokenTemporaryRepository.DoesNotExistAsync(jti, asTracking, cancellationToken);
            if (!doesBlacklistedAccessTokenTemporaryNotExist)
            {
                _logger.LogWarning("ValidateBlacklistedAccessTokenTemporaryAsync: el jti del token temporal es inválido. Jti={Jti}", jti);
                return Result.Fail(Error.Unauthorized("El jti del token temporal es inválido."));
            }
            return Result.Ok();
        }

        public async Task<Result> ValidateUserAsync(int userId, string securityStamp, int tokenVersion, DateTime utcNow, bool asTracking, CancellationToken cancellationToken)
        {
            var isUserActiveAndUnlocked = await _userRepository.IsActiveAndUnlockedAsync(userId, utcNow, asTracking, cancellationToken);
            if (!isUserActiveAndUnlocked)
            {
                _logger.LogWarning("ValidateUserAsync: usuario inválido o bloqueado. UserId={UserId}", userId);
                return Result.Fail(Error.Unauthorized("Usuario inválido o bloqueado."));
            }
            var userHasValidSecurityCredentials = await _userRepository.HasValidSecurityCredentialsAsync(userId, securityStamp, tokenVersion, asTracking, cancellationToken);
            if (!userHasValidSecurityCredentials)
            {
                _logger.LogWarning("ValidateUserAsync: usuario inválido por cambios en la cuenta. UserId={UserId}", userId);
                return Result.Fail(Error.Unauthorized("Usuario inválido por cambios en la cuenta."));
            }
            return Result.Ok();
        }

        public async Task<Result> ValidateUserDeviceAsync(int userId, int userDeviceId, DateTime utcNow, bool asTracking, CancellationToken cancellationToken)
        {
            var isUserDeviceActiveAndUnlocked = await _userDeviceRepository.IsActiveAndUnlockedAsync(userDeviceId, userId, utcNow, asTracking, cancellationToken);
            if (!isUserDeviceActiveAndUnlocked)
            {
                _logger.LogWarning("ValidateUserDeviceAsync: dispositivo inválido o bloqueado. UserId={UserId}, DeviceId={DeviceId}", userId, userDeviceId);
                return Result.Fail(Error.Unauthorized("Dispositivo inválido o bloqueado."));
            }
            return Result.Ok();
        }

        public async Task<Result> ValidateBlacklistedAccessTokenSessionAsync(string jti, bool asTracking, CancellationToken cancellationToken)
        {
            var doesBlacklistedAccessTokenSessionNotExist = await _blacklistedAccessTokenSessionRepository.DoesNotExistAsync(jti, asTracking, cancellationToken);
            if (!doesBlacklistedAccessTokenSessionNotExist)
            {
                _logger.LogWarning("ValidateBlacklistedAccessTokenSessionAsync: el jti del token de sesión es inválido. Jti={Jti}", jti);
                return Result.Fail(Error.Unauthorized("El jti del token de sesión es inválido."));
            }
            return Result.Ok();
        }

        public async Task<Result> ValidateUserWorkProfileAsync(int userId, int workProfileId, WorkProfileType workProfileType, bool asTracking, 
            CancellationToken cancellationToken)
        {
            var isUserWorkProfileActiveAndOfType = await _userWorkProfileRepository.IsActiveAndOfTypeAsync(userId, workProfileId, workProfileType, asTracking, cancellationToken);
            if (!isUserWorkProfileActiveAndOfType)
            {
                _logger.LogWarning("ValidateUserWorkProfileAsync: perfil de trabajo del usuario no encontrado o eliminado. UserId={UserId}, WorkProfileId={WorkProfileId}", userId, 
                    workProfileId);
                return Result.Fail(Error.Unauthorized("No se encontró un perfil de trabajo activo del tipo especificado asignado al usuario."));
            }
            return Result.Ok();
        }

        public async Task<Result> ValidateWorkProfilePermissionsAsync(int workProfileId, bool asTracking, CancellationToken cancellationToken)
        {
            var workProfileHasActivePermissionsWithAccess = await _workProfilePermissionRepository.HasActivePermissionsWithAccessAsync(workProfileId, asTracking, 
                cancellationToken);
            if (!workProfileHasActivePermissionsWithAccess)
            {
                _logger.LogWarning("ValidateWorkProfilePermissionsAsync: perfil de trabajo sin permisos activos. WorkProfileId={WorkProfileId}", workProfileId);
                return Result.Fail(Error.Unauthorized("El perfil de trabajo no tiene permisos activos asignados."));
            }
            return Result.Ok();
        }

        public async Task<Result> ValidateUseRoleCampusAsync(int userId, int roleId, int campusId, bool asTracking, CancellationToken cancellationToken)
        {
            var isUserRoleCampusActive = await _userRoleCampusRepository.IsActiveAsync(userId, roleId, campusId, asTracking, cancellationToken);
            if (!isUserRoleCampusActive)
            {
                _logger.LogWarning("ValidateUseRoleCampusAsync: rol de sede del usuario no encontrado o eliminado. UserId={UserId}, RoleId={RoleId}, CampusId={CampusId}", userId, 
                    roleId, campusId);
                return Result.Fail(Error.Unauthorized("No se encontró un rol de sede del usuario activo."));
            }
            return Result.Ok();
        }

        public async Task<Result> ValidateRolePermissionAsync(int roleId, bool asTracking, CancellationToken cancellationToken)
        {
            var roleHasActivePermissionsWithAccess = await _rolePermissionRepository.HasActivePermissionsWithAccessAsync(roleId, asTracking, cancellationToken);
            if (!roleHasActivePermissionsWithAccess)
            {
                _logger.LogWarning("ValidateRolePermissionAsync: rol sin permisos activos. RoleId={RoleId}", roleId);
                return Result.Fail(Error.Unauthorized("El rol no tiene permisos activos asignados."));
            }
            return Result.Ok();
        }

        public async Task<Result> ValidateAccessTokenTemporaryAsync(string currentJti, int currentUserId, string currentSecurityStamp, int currentTokenVersion,
            int currentUserDeviceId, CancellationToken cancellationToken = default)
        {
            var asTracking = false;
            var utcNow = _dateTimeService.UtcNow;
            var validateBlacklistedAccessTokenTemporaryResult = await ValidateBlacklistedAccessTokenTemporaryAsync(currentJti, asTracking, cancellationToken);
            if (validateBlacklistedAccessTokenTemporaryResult.IsFailure)
                return validateBlacklistedAccessTokenTemporaryResult;
            var validateUserResult = await ValidateUserAsync(currentUserId, currentSecurityStamp, currentTokenVersion, utcNow, asTracking, cancellationToken);
            if (validateUserResult.IsFailure)
                return validateUserResult;
            return await ValidateUserDeviceAsync(currentUserId, currentUserDeviceId, utcNow, asTracking, cancellationToken);
        }

        public async Task<Result> ValidateAccessTokenSessionAsync(string currentJti, int currentUserId, string currentSecurityStamp, int currentTokenVersion, 
            int currentUserDeviceId, int currentUserSessionId, int currentWorkProfileId, WorkProfileType workProfileType, int currentRoleId, int currentCampusId,
            CancellationToken cancellationToken = default)
        {
            var asTracking = false;
            var utcNow = _dateTimeService.UtcNow;
            var validateBlacklistedAccessTokenSessionResult = await ValidateBlacklistedAccessTokenSessionAsync(currentJti, asTracking, cancellationToken);
            if (validateBlacklistedAccessTokenSessionResult.IsFailure)
                return validateBlacklistedAccessTokenSessionResult;
            var validateUserResult = await ValidateUserAsync(currentUserId, currentSecurityStamp, currentTokenVersion, utcNow, asTracking, cancellationToken);
            if (validateUserResult.IsFailure)
                return validateUserResult;
            var validateUserDeviceResult = await ValidateUserDeviceAsync(currentUserId, currentUserDeviceId, utcNow, asTracking, cancellationToken);
            if (validateUserDeviceResult.IsFailure)
                return validateUserDeviceResult;
            var isUserSessionUnRevokedAndUnExpired = await _userSessionRepository.IsUnRevokedAndUnExpiredAsync(currentUserSessionId, currentUserId, utcNow, asTracking,
                cancellationToken);
            if (!isUserSessionUnRevokedAndUnExpired)
            {
                _logger.LogWarning("ValidateCurrentSessionAsync: sesión de usuario revocada o caducada. UserId={UserId}, UserSessionId={UserSessionId}", currentUserId,
                    currentUserSessionId);
                return Result.Fail(Error.Unauthorized("Sesión de Usuario revocada o caducada."));
            }
            /*
             * AQUI REALIZARE
             * OTRAS VALIDACIONES CON:
             * currentWorkProfileId, currentRoleId y currentCampusId
             */
            return Result.Ok();
        }
    }
}