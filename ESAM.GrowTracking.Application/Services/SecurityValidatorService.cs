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

        public async Task<Result> ValidateAccessTokenTemporaryAsync(string currentJti, int currentUserId, string currentSecurityStamp, int currentTokenVersion,
            int currentUserDeviceId, CancellationToken cancellationToken = default)
        {
            var asTracking = false;
            var utcNow = _dateTimeService.UtcNow;
            var doesBlacklistedAccessTokenTemporaryNotExist = await _blacklistedAccessTokenTemporaryRepository.DoesNotExistAsync(currentJti, asTracking, cancellationToken);
            if (!doesBlacklistedAccessTokenTemporaryNotExist)
            {
                _logger.LogWarning("ValidateAccessTokenTemporaryAsync: el jti del token temporal es inválido. Jti={Jti}", currentJti);
                return Result.Fail(Error.Unauthorized("El jti del token temporal es inválido."));
            }
            return await ValidateUserAndUserDeviceAsync(currentUserId, currentSecurityStamp, currentTokenVersion, currentUserDeviceId, utcNow, asTracking, cancellationToken);
        }

        public async Task<Result> ValidateAccessTokenSessionAsync(string currentJti, int currentUserId, string currentSecurityStamp, int currentTokenVersion,
            int currentUserDeviceId, int currentUserSessionId, int currentUserSessionWorkProfileSelectedId, int currentWorkProfileId, WorkProfileType workProfileType, 
            CancellationToken cancellationToken = default)
        {
            var asTracking = false;
            var utcNow = _dateTimeService.UtcNow;
            var validateBlacklistedAccessTokenSessionAndUserAndUserDeviceAndWorkUserProfileResult =
                await ValidateBlacklistedAccessTokenSessionAndUserAndUserDeviceAndWorkUserProfileAsync(currentJti, currentUserId, currentSecurityStamp, currentTokenVersion,
                    currentUserDeviceId, currentWorkProfileId, workProfileType, utcNow, asTracking, cancellationToken);
            if (validateBlacklistedAccessTokenSessionAndUserAndUserDeviceAndWorkUserProfileResult.IsFailure)
                return validateBlacklistedAccessTokenSessionAndUserAndUserDeviceAndWorkUserProfileResult;
            var workProfileHasActivePermissionsWithAccess = await _workProfilePermissionRepository.HasActivePermissionsWithAccessAsync(currentWorkProfileId, asTracking,
                cancellationToken);
            if (!workProfileHasActivePermissionsWithAccess)
            {
                _logger.LogWarning("ValidateAccessTokenSessionAsync: perfil de trabajo sin permisos activos. WorkProfileId={WorkProfileId}", currentWorkProfileId);
                return Result.Fail(Error.Unauthorized("El perfil de trabajo no tiene permisos activos asignados."));
            }
            var isUserSessionUnRevokedAndUnExpiredForWorkProfile = await _userSessionRepository.IsUnRevokedAndUnExpiredForWorkProfileAsync(currentUserSessionId, currentUserId,
                currentUserSessionWorkProfileSelectedId, currentWorkProfileId, utcNow, asTracking, cancellationToken);
            if (!isUserSessionUnRevokedAndUnExpiredForWorkProfile)
            {
                _logger.LogWarning("ValidateCurrentSessionAsync: sesión de usuario inválida, caducada o perfil de trabajo no coincidente. " +
                    "UserId={UserId}, UserSessionId={UserSessionId}, WorkProfileId={WorkProfileId}", currentUserId, currentUserSessionId, currentWorkProfileId);
                return Result.Fail(Error.Unauthorized("La sesión de usuario ha caducado, es inválida o no corresponde al perfil de trabajo seleccionado."));
            }
            return Result.Ok();
        }

        public async Task<Result> ValidateAccessTokenSessionAsync(string currentJti, int currentUserId, string currentSecurityStamp, int currentTokenVersion,
            int currentUserDeviceId, int currentUserSessionId, int currentUserSessionWorkProfileSelectedId, int currentWorkProfileId, WorkProfileType currentWorkProfileType, 
            int currentUserSessionRoleCampusSelectedId, int currentRoleId, int currentCampusId, CancellationToken cancellationToken = default)
        {
            var asTracking = false;
            var utcNow = _dateTimeService.UtcNow;
            var validateBlacklistedAccessTokenSessionAndUserAndUserDeviceAndWorkUserProfileResult =
                await ValidateBlacklistedAccessTokenSessionAndUserAndUserDeviceAndWorkUserProfileAsync(currentJti, currentUserId, currentSecurityStamp, currentTokenVersion,
                    currentUserDeviceId, currentWorkProfileId, currentWorkProfileType, utcNow, asTracking, cancellationToken);
            if (validateBlacklistedAccessTokenSessionAndUserAndUserDeviceAndWorkUserProfileResult.IsFailure)
                return validateBlacklistedAccessTokenSessionAndUserAndUserDeviceAndWorkUserProfileResult;
            var isUserRoleCampusActive = await _userRoleCampusRepository.IsActiveAsync(currentUserId, currentRoleId, currentCampusId, asTracking, cancellationToken);
            if (!isUserRoleCampusActive)
            {
                _logger.LogWarning("ValidateAccessTokenSessionAsync: rol de sede del usuario no encontrado o eliminado. UserId={UserId}, RoleId={RoleId}, CampusId={CampusId}",
                    currentUserId, currentRoleId, currentCampusId);
                return Result.Fail(Error.Unauthorized("No se encontró un rol de sede del usuario activo."));
            }
            var roleHasActivePermissionsWithAccess = await _rolePermissionRepository.HasActivePermissionsWithAccessAsync(currentRoleId, asTracking, cancellationToken);
            if (!roleHasActivePermissionsWithAccess)
            {
                _logger.LogWarning("ValidateAccessTokenSessionAsync: rol sin permisos activos. RoleId={RoleId}", currentRoleId);
                return Result.Fail(Error.Unauthorized("El rol no tiene permisos activos asignados."));
            }
            var isUserSessionUnRevokedAndUnExpiredForRoleCampus = await _userSessionRepository.IsUnRevokedAndUnExpiredForRoleCampusAsync(currentUserSessionId, currentUserId,
                currentUserSessionWorkProfileSelectedId, currentWorkProfileId, currentUserSessionRoleCampusSelectedId, currentRoleId, currentCampusId, utcNow, asTracking, 
                cancellationToken);
            if (!isUserSessionUnRevokedAndUnExpiredForRoleCampus)
            {
                _logger.LogWarning("ValidateCurrentSessionAsync: sesión de usuarui inválida, caducada o contexto de Rol/Sede no coincidente. " +
                    "UserId={UserId}, UserSessionId={UserSessionId}, WorkProfileId={WorkProfileId}, RoleId={RoleId}, CampusId={CampusId}", currentUserId, currentUserSessionId,
                    currentWorkProfileId, currentRoleId, currentCampusId);
                return Result.Fail(Error.Unauthorized("La sesión de usauio ha caducado, es inválida o no coincide con el rol y sede seleccionado."));
            }
            return Result.Ok();
        }

        private async Task<Result> ValidateUserAndUserDeviceAsync(int currentUserId, string currentSecurityStamp, int currentTokenVersion, int currentUserDeviceId, DateTime utcNow,
            bool asTracking, CancellationToken cancellationToken)
        {
            var isUserActiveAndUnlocked = await _userRepository.IsActiveAndUnlockedAsync(currentUserId, utcNow, asTracking, cancellationToken);
            if (!isUserActiveAndUnlocked)
            {
                _logger.LogWarning("ValidateUserAndUserDeviceAsync: usuario inválido o bloqueado. UserId={UserId}", currentUserId);
                return Result.Fail(Error.Unauthorized("Usuario inválido o bloqueado."));
            }
            var userHasValidSecurityCredentials = await _userRepository.HasValidSecurityCredentialsAsync(currentUserId, currentSecurityStamp, currentTokenVersion, asTracking,
                cancellationToken);
            if (!userHasValidSecurityCredentials)
            {
                _logger.LogWarning("ValidateUserAndUserDeviceAsync: usuario inválido por cambios en la cuenta. UserId={UserId}", currentUserId);
                return Result.Fail(Error.Unauthorized("Usuario inválido por cambios en la cuenta."));
            }
            var isUserDeviceActiveAndUnlocked = await _userDeviceRepository.IsActiveAndUnlockedAsync(currentUserDeviceId, currentUserId, utcNow, asTracking, cancellationToken);
            if (!isUserDeviceActiveAndUnlocked)
            {
                _logger.LogWarning("ValidateUserAndUserDeviceAsync: dispositivo de usuario inválido o bloqueado. UserId={UserId}, UserDeviceId={UserDeviceId}", currentUserId,
                    currentUserDeviceId);
                return Result.Fail(Error.Unauthorized("Dispositivo de usuario inválido o bloqueado."));
            }
            return Result.Ok();
        }

        private async Task<Result> ValidateBlacklistedAccessTokenSessionAndUserAndUserDeviceAndWorkUserProfileAsync(string currentJti, int currentUserId,
            string currentSecurityStamp, int currentTokenVersion, int currentUserDeviceId, int currentWorkProfileId, WorkProfileType currentWorkProfileType, DateTime utcNow,
            bool asTracking, CancellationToken cancellationToken)
        {
            var doesBlacklistedAccessTokenSessionNotExist = await _blacklistedAccessTokenSessionRepository.DoesNotExistAsync(currentJti, asTracking, cancellationToken);
            if (!doesBlacklistedAccessTokenSessionNotExist)
            {
                _logger.LogWarning("ValidateBlacklistedAccessTokenSessionAndUserAndUserDeviceAndWorkUserProfileAsync: el jti del token de sesión es inválido. Jti={Jti}",
                    currentJti);
                return Result.Fail(Error.Unauthorized("El jti del token de sesión es inválido."));
            }
            var validateUserAndUserDeviceResult = await ValidateUserAndUserDeviceAsync(currentUserId, currentSecurityStamp, currentTokenVersion, currentUserDeviceId, utcNow,
                asTracking, cancellationToken);
            if (validateUserAndUserDeviceResult.IsFailure)
                return validateUserAndUserDeviceResult;
            var isUserWorkProfileActiveAndOfType = await _userWorkProfileRepository.IsActiveAndOfTypeAsync(currentUserId, currentWorkProfileId, currentWorkProfileType,
                asTracking, cancellationToken);
            if (!isUserWorkProfileActiveAndOfType)
            {
                _logger.LogWarning("ValidateBlacklistedAccessTokenSessionAndUserAndUserDeviceAndWorkUserProfileAsync: perfil de trabajo del usuario no encontrado o eliminado. " +
                    " UserId={UserId}, WorkProfileId={WorkProfileId}", currentUserId, currentWorkProfileId);
                return Result.Fail(Error.Unauthorized("No se encontró un perfil de trabajo activo del tipo especificado asignado al usuario."));
            }
            return Result.Ok();
        }
    }

    //public sealed class SecurityValidatorService : ISecurityValidatorService
    //{
    //    private readonly ILogger<SecurityValidatorService> _logger;
    //    private readonly IBlacklistedAccessTokenTemporaryRepository _blacklistedAccessTokenTemporaryRepository;
    //    private readonly IUserRepository _userRepository;
    //    private readonly IUserDeviceRepository _userDeviceRepository;
    //    private readonly IBlacklistedAccessTokenSessionRepository _blacklistedAccessTokenSessionRepository;
    //    private readonly IUserWorkProfileRepository _userWorkProfileRepository;
    //    private readonly IWorkProfilePermissionRepository _workProfilePermissionRepository;
    //    private readonly IUserRoleCampusRepository _userRoleCampusRepository;
    //    private readonly IRolePermissionRepository _rolePermissionRepository;
    //    private readonly IUserSessionRepository _userSessionRepository;
    //    private readonly IDateTimeService _dateTimeService;

    //    public SecurityValidatorService(ILogger<SecurityValidatorService> logger, IBlacklistedAccessTokenTemporaryRepository blacklistedAccessTokenTemporaryRepository, 
    //        IUserRepository userRepository, IUserDeviceRepository userDeviceRepository, IBlacklistedAccessTokenSessionRepository blacklistedAccessTokenSessionRepository, 
    //        IUserWorkProfileRepository userWorkProfileRepository, IWorkProfilePermissionRepository workProfilePermissionRepository, 
    //        IUserRoleCampusRepository userRoleCampusRepository, IRolePermissionRepository rolePermissionRepository, IUserSessionRepository userSessionRepository, 
    //        IDateTimeService dateTimeService)
    //    {
    //        ArgumentNullException.ThrowIfNull(logger);
    //        ArgumentNullException.ThrowIfNull(blacklistedAccessTokenTemporaryRepository);
    //        ArgumentNullException.ThrowIfNull(userRepository);
    //        ArgumentNullException.ThrowIfNull(userDeviceRepository);
    //        ArgumentNullException.ThrowIfNull(blacklistedAccessTokenSessionRepository);
    //        ArgumentNullException.ThrowIfNull(userWorkProfileRepository);
    //        ArgumentNullException.ThrowIfNull(workProfilePermissionRepository);
    //        ArgumentNullException.ThrowIfNull(userRoleCampusRepository);
    //        ArgumentNullException.ThrowIfNull(rolePermissionRepository);
    //        ArgumentNullException.ThrowIfNull(userSessionRepository);
    //        ArgumentNullException.ThrowIfNull(dateTimeService);
    //        _logger = logger;
    //        _blacklistedAccessTokenTemporaryRepository = blacklistedAccessTokenTemporaryRepository;
    //        _userRepository = userRepository;
    //        _userDeviceRepository = userDeviceRepository;
    //        _blacklistedAccessTokenSessionRepository = blacklistedAccessTokenSessionRepository;
    //        _userWorkProfileRepository = userWorkProfileRepository;
    //        _workProfilePermissionRepository = workProfilePermissionRepository;
    //        _userRoleCampusRepository = userRoleCampusRepository;
    //        _rolePermissionRepository = rolePermissionRepository;
    //        _userSessionRepository = userSessionRepository;
    //        _dateTimeService = dateTimeService;
    //    }

    //    public async Task<Result> ValidateAccessTokenTemporaryAsync(string currentJti, int currentUserId, string currentSecurityStamp, int currentTokenVersion,
    //        int currentUserDeviceId, CancellationToken cancellationToken = default)
    //    {
    //        var asTracking = false;
    //        var utcNow = _dateTimeService.UtcNow;
    //        var doesBlacklistedAccessTokenTemporaryNotExist = await _blacklistedAccessTokenTemporaryRepository.DoesNotExistAsync(currentJti, asTracking, cancellationToken);
    //        if (!doesBlacklistedAccessTokenTemporaryNotExist)
    //        {
    //            _logger.LogWarning("ValidateAccessTokenTemporaryAsync: el jti del token temporal es inválido. Jti={Jti}", currentJti);
    //            return Result.Fail(Error.Unauthorized("El jti del token temporal es inválido."));
    //        }
    //        return await ValidateUserAndUserDeviceAsync(currentUserId, currentSecurityStamp, currentTokenVersion, currentUserDeviceId, utcNow, asTracking, cancellationToken);
    //    }

    //    public async Task<Result> ValidateAccessTokenSessionAsync(string currentJti, int currentUserId, string currentSecurityStamp, int currentTokenVersion,
    //        int currentUserDeviceId, int currentUserSessionId, int currentWorkProfileId, WorkProfileType workProfileType, CancellationToken cancellationToken = default)
    //    {
    //        var asTracking = false;
    //        var utcNow = _dateTimeService.UtcNow;
    //        var validateBlacklistedAccessTokenSessionAndUserAndUserDeviceAndWorkUserProfileResult =
    //            await ValidateBlacklistedAccessTokenSessionAndUserAndUserDeviceAndWorkUserProfileAsync(currentJti, currentUserId, currentSecurityStamp, currentTokenVersion,
    //                currentUserDeviceId, currentWorkProfileId, workProfileType, utcNow, asTracking, cancellationToken);
    //        if (validateBlacklistedAccessTokenSessionAndUserAndUserDeviceAndWorkUserProfileResult.IsFailure)
    //            return validateBlacklistedAccessTokenSessionAndUserAndUserDeviceAndWorkUserProfileResult;
    //        var workProfileHasActivePermissionsWithAccess = await _workProfilePermissionRepository.HasActivePermissionsWithAccessAsync(currentWorkProfileId, asTracking,
    //            cancellationToken);
    //        if (!workProfileHasActivePermissionsWithAccess)
    //        {
    //            _logger.LogWarning("ValidateAccessTokenSessionAsync: perfil de trabajo sin permisos activos. WorkProfileId={WorkProfileId}", currentWorkProfileId);
    //            return Result.Fail(Error.Unauthorized("El perfil de trabajo no tiene permisos activos asignados."));
    //        }
    //        var isUserSessionUnRevokedAndUnExpiredForWorkProfile = await _userSessionRepository.IsUnRevokedAndUnExpiredForWorkProfileAsync(currentUserSessionId, currentUserId, 
    //            currentWorkProfileId, utcNow, asTracking, cancellationToken);
    //        if (!isUserSessionUnRevokedAndUnExpiredForWorkProfile)
    //        {
    //            _logger.LogWarning("ValidateCurrentSessionAsync: sesión de usuario inválida, caducada o perfil de trabajo no coincidente. " + 
    //                "UserId={UserId}, UserSessionId={UserSessionId}, WorkProfileId={WorkProfileId}", currentUserId, currentUserSessionId, currentWorkProfileId);
    //            return Result.Fail(Error.Unauthorized("La sesión de usuario ha caducado, es inválida o no corresponde al perfil de trabajo seleccionado."));
    //        }
    //        return Result.Ok();
    //    }

    //    public async Task<Result> ValidateAccessTokenSessionAsync(string currentJti, int currentUserId, string currentSecurityStamp, int currentTokenVersion, 
    //        int currentUserDeviceId, int currentUserSessionId, int currentWorkProfileId, WorkProfileType currentWorkProfileType, int currentRoleId, int currentCampusId,
    //        CancellationToken cancellationToken = default)
    //    {
    //        var asTracking = false;
    //        var utcNow = _dateTimeService.UtcNow;
    //        var validateBlacklistedAccessTokenSessionAndUserAndUserDeviceAndWorkUserProfileResult =
    //            await ValidateBlacklistedAccessTokenSessionAndUserAndUserDeviceAndWorkUserProfileAsync(currentJti, currentUserId, currentSecurityStamp, currentTokenVersion,
    //                currentUserDeviceId, currentWorkProfileId, currentWorkProfileType, utcNow, asTracking, cancellationToken);
    //        if (validateBlacklistedAccessTokenSessionAndUserAndUserDeviceAndWorkUserProfileResult.IsFailure)
    //            return validateBlacklistedAccessTokenSessionAndUserAndUserDeviceAndWorkUserProfileResult;
    //        var isUserRoleCampusActive = await _userRoleCampusRepository.IsActiveAsync(currentUserId, currentRoleId, currentCampusId, asTracking, cancellationToken);
    //        if (!isUserRoleCampusActive)
    //        {
    //            _logger.LogWarning("ValidateAccessTokenSessionAsync: rol de sede del usuario no encontrado o eliminado. UserId={UserId}, RoleId={RoleId}, CampusId={CampusId}", 
    //                currentUserId, currentRoleId, currentCampusId);
    //            return Result.Fail(Error.Unauthorized("No se encontró un rol de sede del usuario activo."));
    //        }
    //        var roleHasActivePermissionsWithAccess = await _rolePermissionRepository.HasActivePermissionsWithAccessAsync(currentRoleId, asTracking, cancellationToken);
    //        if (!roleHasActivePermissionsWithAccess)
    //        {
    //            _logger.LogWarning("ValidateAccessTokenSessionAsync: rol sin permisos activos. RoleId={RoleId}", currentRoleId);
    //            return Result.Fail(Error.Unauthorized("El rol no tiene permisos activos asignados."));
    //        }
    //        var isUserSessionUnRevokedAndUnExpiredForRoleCampus = await _userSessionRepository.IsUnRevokedAndUnExpiredForRoleCampusAsync(currentUserSessionId, currentUserId,
    //            currentWorkProfileId, currentRoleId, currentCampusId, utcNow, asTracking, cancellationToken);
    //        if (!isUserSessionUnRevokedAndUnExpiredForRoleCampus)
    //        {
    //            _logger.LogWarning("ValidateCurrentSessionAsync: sesión de usuarui inválida, caducada o contexto de Rol/Sede no coincidente. " + 
    //                "UserId={UserId}, UserSessionId={UserSessionId}, WorkProfileId={WorkProfileId}, RoleId={RoleId}, CampusId={CampusId}", currentUserId, currentUserSessionId, 
    //                currentWorkProfileId, currentRoleId, currentCampusId);
    //            return Result.Fail(Error.Unauthorized("La sesión de usauio ha caducado, es inválida o no coincide con el rol y sede seleccionado."));
    //        }
    //        return Result.Ok();
    //    }

    //    private async Task<Result> ValidateUserAndUserDeviceAsync(int currentUserId, string currentSecurityStamp, int currentTokenVersion, int currentUserDeviceId, DateTime utcNow, 
    //        bool asTracking, CancellationToken cancellationToken)
    //    {
    //        var isUserActiveAndUnlocked = await _userRepository.IsActiveAndUnlockedAsync(currentUserId, utcNow, asTracking, cancellationToken);
    //        if (!isUserActiveAndUnlocked)
    //        {
    //            _logger.LogWarning("ValidateUserAndUserDeviceAsync: usuario inválido o bloqueado. UserId={UserId}", currentUserId);
    //            return Result.Fail(Error.Unauthorized("Usuario inválido o bloqueado."));
    //        }
    //        var userHasValidSecurityCredentials = await _userRepository.HasValidSecurityCredentialsAsync(currentUserId, currentSecurityStamp, currentTokenVersion, asTracking, 
    //            cancellationToken);
    //        if (!userHasValidSecurityCredentials)
    //        {
    //            _logger.LogWarning("ValidateUserAndUserDeviceAsync: usuario inválido por cambios en la cuenta. UserId={UserId}", currentUserId);
    //            return Result.Fail(Error.Unauthorized("Usuario inválido por cambios en la cuenta."));
    //        }
    //        var isUserDeviceActiveAndUnlocked = await _userDeviceRepository.IsActiveAndUnlockedAsync(currentUserDeviceId, currentUserId, utcNow, asTracking, cancellationToken);
    //        if (!isUserDeviceActiveAndUnlocked)
    //        {
    //            _logger.LogWarning("ValidateUserAndUserDeviceAsync: dispositivo de usuario inválido o bloqueado. UserId={UserId}, UserDeviceId={UserDeviceId}", currentUserId, 
    //                currentUserDeviceId);
    //            return Result.Fail(Error.Unauthorized("Dispositivo de usuario inválido o bloqueado."));
    //        }
    //        return Result.Ok();
    //    }

    //    private async Task<Result> ValidateBlacklistedAccessTokenSessionAndUserAndUserDeviceAndWorkUserProfileAsync(string currentJti, int currentUserId, 
    //        string currentSecurityStamp, int currentTokenVersion, int currentUserDeviceId, int currentWorkProfileId, WorkProfileType currentWorkProfileType, DateTime utcNow, 
    //        bool asTracking, CancellationToken cancellationToken)
    //    {
    //        var doesBlacklistedAccessTokenSessionNotExist = await _blacklistedAccessTokenSessionRepository.DoesNotExistAsync(currentJti, asTracking, cancellationToken);
    //        if (!doesBlacklistedAccessTokenSessionNotExist)
    //        {
    //            _logger.LogWarning("ValidateBlacklistedAccessTokenSessionAndUserAndUserDeviceAndWorkUserProfileAsync: el jti del token de sesión es inválido. Jti={Jti}", 
    //                currentJti);
    //            return Result.Fail(Error.Unauthorized("El jti del token de sesión es inválido."));
    //        }
    //        var validateUserAndUserDeviceResult = await ValidateUserAndUserDeviceAsync(currentUserId, currentSecurityStamp, currentTokenVersion, currentUserDeviceId, utcNow, 
    //            asTracking, cancellationToken);
    //        if (validateUserAndUserDeviceResult.IsFailure)
    //            return validateUserAndUserDeviceResult;
    //        var isUserWorkProfileActiveAndOfType = await _userWorkProfileRepository.IsActiveAndOfTypeAsync(currentUserId, currentWorkProfileId, currentWorkProfileType, 
    //            asTracking, cancellationToken);
    //        if (!isUserWorkProfileActiveAndOfType)
    //        {
    //            _logger.LogWarning("ValidateBlacklistedAccessTokenSessionAndUserAndUserDeviceAndWorkUserProfileAsync: perfil de trabajo del usuario no encontrado o eliminado. " + 
    //                " UserId={UserId}, WorkProfileId={WorkProfileId}", currentUserId, currentWorkProfileId);
    //            return Result.Fail(Error.Unauthorized("No se encontró un perfil de trabajo activo del tipo especificado asignado al usuario."));
    //        }
    //        return Result.Ok();
    //    }
    //}
}