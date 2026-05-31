using ESAM.GrowTracking.Application.Abstractions.Services;
using ESAM.GrowTracking.Application.Results;
using ESAM.GrowTracking.Application.ValueObjects;
using ESAM.GrowTracking.Domain.Abstractions.DataAccess.Repositories;
using ESAM.GrowTracking.Domain.Entities;
using ESAM.GrowTracking.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace ESAM.GrowTracking.Application.Services
{
    public class CurrentUserValidatorService : ICurrentUserValidatorService
    {
        private readonly ILogger<CurrentUserValidatorService> _logger;
        private readonly ICurrentUserService _currentUserService;
        private readonly IUserRepository _userRepository;
        private readonly IUserDeviceRepository _userDeviceRepository;
        private readonly IUserWorkProfileRepository _userWorkProfileRepository;
        private readonly IWorkProfileRepository _workProfileRepository;
        private readonly IWorkProfilePermissionRepository _workProfilePermissionRepository;
        private readonly IUserRoleCampusRepository _userRoleCampusRepository;
        private readonly IRolePermissionRepository _rolePermissionRepository;

        public CurrentUserValidatorService(ILogger<CurrentUserValidatorService> logger, ICurrentUserService currentUserService, IUserRepository userRepository,
            IUserDeviceRepository userDeviceRepository, IUserWorkProfileRepository userWorkProfileRepository, IWorkProfileRepository workProfileRepository,
            IWorkProfilePermissionRepository workProfilePermissionRepository, IUserRoleCampusRepository userRoleCampusRepository,
            IRolePermissionRepository rolePermissionRepository)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(currentUserService);
            ArgumentNullException.ThrowIfNull(userRepository);
            ArgumentNullException.ThrowIfNull(userDeviceRepository);
            ArgumentNullException.ThrowIfNull(userWorkProfileRepository);
            ArgumentNullException.ThrowIfNull(workProfileRepository);
            ArgumentNullException.ThrowIfNull(workProfilePermissionRepository);
            ArgumentNullException.ThrowIfNull(userRoleCampusRepository);
            ArgumentNullException.ThrowIfNull(rolePermissionRepository);
            _logger = logger;
            _currentUserService = currentUserService;
            _userRepository = userRepository;
            _userDeviceRepository = userDeviceRepository;
            _userWorkProfileRepository = userWorkProfileRepository;
            _workProfileRepository = workProfileRepository;
            _workProfilePermissionRepository = workProfilePermissionRepository;
            _userRoleCampusRepository = userRoleCampusRepository;
            _rolePermissionRepository = rolePermissionRepository;
        }

        private async Task<(User? User, Result? Failure)> GetAndValidateUserCoreAsync(int currentUserId, DateTime utcNow, bool asTracking, CancellationToken cancellationToken)
        { 
            var currentSecurityStamp = _currentUserService.SecurityStamp!;
            var currentTokenVersion = _currentUserService.TokenVersion!.Value;
            var user = await _userRepository.GetByIdAsync(currentUserId, asTracking, cancellationToken);
            if (user is null || user.IsDeleted || user.IsLocked(utcNow))
            {
                _logger.LogWarning("CurrentUserValidatorService: usuario inválido o bloqueado. UserId={UserId}", currentUserId);
                return (null, Result.Fail(Error.Unauthorized("Usuario inválido o bloqueado.")));
            }
            if (user.SecurityStamp != currentSecurityStamp || user.TokenVersion != currentTokenVersion)
            {
                _logger.LogWarning("CurrentUserValidatorService: sesión invalidada por cambios en la cuenta. UserId={UserId}", currentUserId);
                return (null, Result.Fail(Error.Unauthorized("Sesión invalidada por cambios en la cuenta.")));
            }
            return (user, null);
        }

        public async Task<Result> ValidateCurrentUserAsync(int currentUserId, DateTime utcNow, bool asTracking = false, CancellationToken cancellationToken = default)
        {
            var (_, failure) = await GetAndValidateUserCoreAsync(currentUserId, utcNow, asTracking, cancellationToken);
            return failure ?? Result.Ok();
        }

        public async Task<Result<User>> GetAndValidateCurrentUserAsync(int currentUserId, DateTime utcNow, bool asTracking = false, CancellationToken cancellationToken = default)
        {
            var (user, failure) = await GetAndValidateUserCoreAsync(currentUserId, utcNow, asTracking, cancellationToken);
            if (failure is not null)
                return Result<User>.Fail(failure.Errors);
            return Result<User>.Ok(user!);
        }

        public async Task<Result> ValidateCurrentUserDeviceAsync(int currentUserId, int currentUserDeviceId, DateTime utcNow, bool asTracking = false,
            CancellationToken cancellationToken = default)
        {
            var userDevice = await _userDeviceRepository.GetByIdAndUserIdAsync(currentUserDeviceId, currentUserId, asTracking, cancellationToken);
            if (userDevice is null || userDevice.IsDeleted || userDevice.IsLocked(utcNow))
            {
                _logger.LogWarning("ValidateCurrentUserDeviceAsync: dispositivo inválido o bloqueado. UserId={UserId}, DeviceId={DeviceId}", currentUserId, currentUserDeviceId);
                return Result.Fail(Error.Unauthorized("Dispositivo inválido o bloqueado."));
            }
            return Result.Ok();
        }

        public async Task<Result<UserDevice>> GetAndValidateCurrentUserDeviceAsync(int currentUserId, int currentUserDeviceId, DateTime utcNow, bool asTracking = false,
            CancellationToken cancellationToken = default)
        {
            var userDevice = await _userDeviceRepository.GetByIdAndUserIdAsync(currentUserDeviceId, currentUserId, asTracking, cancellationToken);
            if (userDevice is null || userDevice.IsDeleted || userDevice.IsLocked(utcNow))
            {
                _logger.LogWarning("GetAndValidateCurrentUserDeviceAsync: dispositivo inválido o bloqueado. UserId={UserId}, DeviceId={DeviceId}", currentUserId, 
                    currentUserDeviceId);
                return Result<UserDevice>.Fail(Error.Unauthorized("Dispositivo inválido o bloqueado."));
            }
            return Result<UserDevice>.Ok(userDevice);
        }

        public async Task<Result> ValidateUserWorkProfileAndTypeAsync(int currentUserId, int currentWorkProfileId, WorkProfileType workProfileType, bool asTracking = false,
            CancellationToken cancellationToken = default)
        {
            var userWorkProfile = await _userWorkProfileRepository.GetByUserIdAndWorkProfileIdAsync(currentUserId, currentWorkProfileId, asTracking, cancellationToken);
            if (userWorkProfile is null || userWorkProfile.IsDeleted)
            {
                _logger.LogWarning("ValidateUserWorkProfileAndTypeAsync: perfil de trabajo no encontrado o eliminado. UserId={UserId}, WorkProfileId={WorkProfileId}",
                    currentUserId, currentWorkProfileId);
                return Result.Fail(Error.NotFound("No se encontró un perfil de trabajo activo asignado al usuario."));
            }
            var isValidWorkProfileType = await _workProfileRepository.IsValidWorkProfileTypeAsync(currentWorkProfileId, workProfileType, asTracking, cancellationToken);
            if (!isValidWorkProfileType)
            {
                _logger.LogWarning("ValidateUserWorkProfileAndTypeAsync: tipo de perfil de trabajo inválido. WorkProfileId={WorkProfileId}, TipoEsperado={ExpectedType}",
                    currentWorkProfileId, workProfileType);
                return Result.Fail(Error.BusinessRule("El perfil de trabajo no corresponde al tipo esperado."));
            }
            return Result.Ok();
        }

        public async Task<Result> ValidateUserWorkProfileAndTypeAndHasPermissionsAsync(int currentUserId, int currentWorkProfileId, WorkProfileType workProfileType,
            bool asTracking = false, CancellationToken cancellationToken = default)
        {
            var workProfileTypeValidationResult = await ValidateUserWorkProfileAndTypeAsync(currentUserId, currentWorkProfileId, workProfileType, asTracking, cancellationToken);
            if (workProfileTypeValidationResult.IsFailure)
                return Result.Fail(workProfileTypeValidationResult.Errors);
            var hasActivePermissions = await _workProfilePermissionRepository.HasActivePermissionsAsync(currentWorkProfileId, asTracking, cancellationToken);
            if (!hasActivePermissions)
            {
                _logger.LogWarning("ValidateUserWorkProfileAndTypeAndHasPermissionsAsync: perfil de trabajo sin permisos activos. WorkProfileId={WorkProfileId}",
                    currentWorkProfileId);
                return Result.Fail(Error.Forbidden("El perfil de trabajo no tiene permisos activos asignados."));
            }
            return Result.Ok();
        }

        public async Task<Result> ValidateUserRoleCampusAndHasPermissionsAsync(int currentUserId, int currentRoleId, int currentCampusId, bool asTracking = false,
            CancellationToken cancellationToken = default)
        {
            var userRoleCampus = await _userRoleCampusRepository.GetByUserIdAndRoleIdAndCampusIdAsync(currentUserId, currentRoleId, currentCampusId, asTracking, cancellationToken);
            if (userRoleCampus is null || userRoleCampus.IsDeleted)
            {
                _logger.LogWarning("ValidateUserRoleCampusAndHasPermissionsAsync: rol de sede no encontrado o eliminado. UserId={UserId}, RoleId={RoleId}, CampusId={CampusId}",
                    currentUserId, currentRoleId, currentCampusId);
                return Result.Fail(Error.NotFound("No se encontró un rol de sede activo asignado al usuario."));
            }
            var hasActivePermissions = await _rolePermissionRepository.HasActivePermissionsAsync(userRoleCampus.RoleId, asTracking, cancellationToken);
            if (!hasActivePermissions)
            {
                _logger.LogWarning("ValidateUserRoleCampusAndHasPermissionsAsync: el rol no tiene permisos activos. RoleId={RoleId}", userRoleCampus.RoleId);
                return Result.Fail(Error.Forbidden("El rol no tiene permisos activos asignados."));
            }
            return Result.Ok();
        }
    }

    //public class CurrentUserValidatorService : ICurrentUserValidatorService
    //{
    //    private readonly ILogger<CurrentUserValidatorService> _logger;
    //    private readonly ICurrentUserService _currentUserService;
    //    private readonly IUserRepository _userRepository;
    //    private readonly IUserDeviceRepository _userDeviceRepository;
    //    private readonly IUserWorkProfileRepository _userWorkProfileRepository;
    //    private readonly IWorkProfileRepository _workProfileRepository;
    //    private readonly IWorkProfilePermissionRepository _workProfilePermissionRepository;
    //    private readonly IUserRoleCampusRepository _userRoleCampusRepository;
    //    private readonly IRolePermissionRepository _rolePermissionRepository;

    //    public CurrentUserValidatorService(ILogger<CurrentUserValidatorService> logger, ICurrentUserService currentUserService, IUserRepository userRepository,
    //        IUserDeviceRepository userDeviceRepository, IUserWorkProfileRepository userWorkProfileRepository, IWorkProfileRepository workProfileRepository,
    //        IWorkProfilePermissionRepository workProfilePermissionRepository, IUserRoleCampusRepository userRoleCampusRepository,
    //        IRolePermissionRepository rolePermissionRepository)
    //    {
    //        ArgumentNullException.ThrowIfNull(logger);
    //        ArgumentNullException.ThrowIfNull(currentUserService);
    //        ArgumentNullException.ThrowIfNull(userRepository);
    //        ArgumentNullException.ThrowIfNull(userDeviceRepository);
    //        ArgumentNullException.ThrowIfNull(userWorkProfileRepository);
    //        ArgumentNullException.ThrowIfNull(workProfileRepository);
    //        ArgumentNullException.ThrowIfNull(workProfilePermissionRepository);
    //        ArgumentNullException.ThrowIfNull(userRoleCampusRepository);
    //        ArgumentNullException.ThrowIfNull(rolePermissionRepository);
    //        _logger = logger;
    //        _currentUserService = currentUserService;
    //        _userRepository = userRepository;
    //        _userDeviceRepository = userDeviceRepository;
    //        _userWorkProfileRepository = userWorkProfileRepository;
    //        _workProfileRepository = workProfileRepository;
    //        _workProfilePermissionRepository = workProfilePermissionRepository;
    //        _userRoleCampusRepository = userRoleCampusRepository;
    //        _rolePermissionRepository = rolePermissionRepository;
    //    }

    //    public async Task<Result> ValidateCurrentUserAsync(int currentUserId, DateTime utcNow, bool asTracking = false, CancellationToken cancellationToken = default)
    //    {
    //        var currentSecurityStamp = _currentUserService.SecurityStamp!;
    //        var currentTokenVersion = _currentUserService.TokenVersion!.Value;
    //        var user = await _userRepository.GetByIdAsync(currentUserId, asTracking, cancellationToken);
    //        if (user is null || user.IsDeleted || user.IsLocked(utcNow))
    //        {
    //            _logger.LogWarning("ValidateCurrentUserAsync: usuario inválido o bloqueado. UserId={UserId}", currentUserId);
    //            return Result.Fail(Error.Unauthorized("Usuario inválido o bloqueado."));
    //        }
    //        if (user.SecurityStamp != currentSecurityStamp || user.TokenVersion != currentTokenVersion)
    //        {
    //            _logger.LogWarning("ValidateCurrentUserAsync: sesión invalidada por cambios en la cuenta. UserId={UserId}", currentUserId);
    //            return Result.Fail(Error.Unauthorized("Sesión invalidada por cambios en la cuenta."));
    //        }
    //        return Result.Ok();
    //    }

    //    public async Task<Result<User>> GetAndValidateCurrentUserAsync(int currentUserId, DateTime utcNow, bool asTracking = false, CancellationToken cancellationToken = default)
    //    {
    //        var currentSecurityStamp = _currentUserService.SecurityStamp!;
    //        var currentTokenVersion = _currentUserService.TokenVersion!.Value;
    //        var user = await _userRepository.GetByIdAsync(currentUserId, asTracking, cancellationToken);
    //        if (user is null || user.IsDeleted || user.IsLocked(utcNow))
    //        {
    //            _logger.LogWarning("GetAndValidateCurrentUserAsync: usuario inválido o bloqueado. UserId={UserId}", currentUserId);
    //            return Result<User>.Fail(Error.Unauthorized("Usuario inválido o bloqueado."));
    //        }
    //        if (user.SecurityStamp != currentSecurityStamp || user.TokenVersion != currentTokenVersion)
    //        {
    //            _logger.LogWarning("GetAndValidateCurrentUserAsync: sesión invalidada por cambios en la cuenta. UserId={UserId}", currentUserId);
    //            return Result<User>.Fail(Error.Unauthorized("Sesión invalidada por cambios en la cuenta."));
    //        }
    //        return Result<User>.Ok(user);
    //    }

    //    public async Task<Result> ValidateCurrentUserDeviceAsync(int currentUserId, int currentUserDeviceId, DateTime utcNow, bool asTracking = false,
    //        CancellationToken cancellationToken = default)
    //    {
    //        var userDevice = await _userDeviceRepository.GetByIdAndUserIdAsync(currentUserDeviceId, currentUserId, asTracking, cancellationToken);
    //        if (userDevice is null || userDevice.IsDeleted || userDevice.IsLocked(utcNow))
    //        {
    //            _logger.LogWarning("ValidateCurrentUserDeviceAsync: dispositivo inválido o bloqueado. UserId={UserId}, DeviceId={DeviceId}", currentUserId, currentUserDeviceId);
    //            return Result.Fail(Error.Unauthorized("Dispositivo inválido o bloqueado."));
    //        }
    //        return Result.Ok();
    //    }

    //    public async Task<Result<UserDevice>> GetAndValidateCurrentUserDeviceAsync(int currentUserId, int currentUserDeviceId, DateTime utcNow, bool asTracking = false,
    //        CancellationToken cancellationToken = default)
    //    {
    //        var userDevice = await _userDeviceRepository.GetByIdAndUserIdAsync(currentUserDeviceId, currentUserId, asTracking, cancellationToken);
    //        if (userDevice is null || userDevice.IsDeleted || userDevice.IsLocked(utcNow))
    //        {
    //            _logger.LogWarning("GetAndValidateCurrentUserDeviceAsync: dispositivo inválido o bloqueado. UserId={UserId}, DeviceId={DeviceId}", currentUserId,
    //                currentUserDeviceId);
    //            return Result<UserDevice>.Fail(Error.Unauthorized("Dispositivo inválido o bloqueado."));
    //        }
    //        return Result<UserDevice>.Ok(userDevice);
    //    }

    //    public async Task<Result> ValidateUserWorkProfileAndTypeAsync(int currentUserId, int currentWorkProfileId, WorkProfileType workProfileType, bool asTracking = false,
    //        CancellationToken cancellationToken = default)
    //    {
    //        var userWorkProfile = await _userWorkProfileRepository.GetByUserIdAndWorkProfileIdAsync(currentUserId, currentWorkProfileId, asTracking, cancellationToken);
    //        if (userWorkProfile is null || userWorkProfile.IsDeleted)
    //        {
    //            _logger.LogWarning("ValidateUserWorkProfileAndTypeAsync: perfil de trabajo no encontrado o eliminado. UserId={UserId}, WorkProfileId={WorkProfileId}",
    //                currentUserId, currentWorkProfileId);
    //            return Result.Fail(Error.NotFound("No se encontró un perfil de trabajo activo asignado al usuario."));
    //        }
    //        var isValidWorkProfileType = await _workProfileRepository.IsValidWorkProfileTypeAsync(currentWorkProfileId, workProfileType, asTracking, cancellationToken);
    //        if (!isValidWorkProfileType)
    //        {
    //            _logger.LogWarning("ValidateUserWorkProfileAndTypeAsync: tipo de perfil de trabajo inválido. WorkProfileId={WorkProfileId}, TipoEsperado={ExpectedType}",
    //                currentWorkProfileId, workProfileType);
    //            return Result.Fail(Error.Validation("El perfil de trabajo no corresponde al tipo esperado."));
    //        }
    //        return Result.Ok();
    //    }

    //    public async Task<Result> ValidateUserWorkProfileAndTypeAndHasPermissionsAsync(int currentUserId, int currentWorkProfileId, WorkProfileType workProfileType,
    //        bool asTracking = false, CancellationToken cancellationToken = default)
    //    {
    //        var validateUserWorkProfileAndTypeResult = await ValidateUserWorkProfileAndTypeAsync(currentUserId, currentWorkProfileId, workProfileType, asTracking,
    //            cancellationToken);
    //        if (validateUserWorkProfileAndTypeResult.IsFailure)
    //            return Result.Fail(validateUserWorkProfileAndTypeResult.Errors);
    //        var hasActivePermissions = await _workProfilePermissionRepository.HasActivePermissionsAsync(currentWorkProfileId, asTracking, cancellationToken);
    //        if (!hasActivePermissions)
    //        {
    //            _logger.LogWarning("ValidateUserWorkProfileAndTypeAndHasPermissionsAsync: perfil de trabajo sin permisos activos. WorkProfileId={WorkProfileId}",
    //                currentWorkProfileId);
    //            return Result.Fail(Error.NotFound("El perfil de trabajo no tiene permisos activos asignados."));
    //        }
    //        return Result.Ok();
    //    }

    //    public async Task<Result> ValidateUserRoleCampusAndHasPermissionsAsync(int currentUserId, int currentRoleId, int currentCampusId, bool asTracking = false,
    //        CancellationToken cancellationToken = default)
    //    {
    //        var userRoleCampus = await _userRoleCampusRepository.GetByUserIdAndRoleIdAndCampusIdAsync(currentUserId, currentRoleId, currentCampusId, asTracking, cancellationToken);
    //        if (userRoleCampus is null || userRoleCampus.IsDeleted)
    //        {
    //            _logger.LogWarning("ValidateUserRoleCampusAndHasPermissionsAsync: rol de sede no encontrado o eliminado. UserId={UserId}, RoleId={RoleId}, CampusId={CampusId}",
    //                currentUserId, currentRoleId, currentCampusId);
    //            return Result.Fail(Error.NotFound("No se encontró un rol de sede activo asignado al usuario."));
    //        }
    //        var hasActivePermissions = await _rolePermissionRepository.HasActivePermissionsAsync(userRoleCampus.RoleId, asTracking, cancellationToken);
    //        if (!hasActivePermissions)
    //        {
    //            _logger.LogWarning("ValidateUserRoleCampusAndHasPermissionsAsync: el rol no tiene permisos activos. RoleId={RoleId}", userRoleCampus.RoleId);
    //            return Result.Fail(Error.NotFound("El rol no tiene permisos activos asignados."));
    //        }
    //        return Result.Ok();
    //    }
    //}
}