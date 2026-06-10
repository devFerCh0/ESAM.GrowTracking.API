using ESAM.GrowTracking.Application.Abstractions.Services;
using ESAM.GrowTracking.Application.Results;
using ESAM.GrowTracking.Application.ValueObjects;
using ESAM.GrowTracking.Domain.Abstractions.DataAccess.Repositories;
using ESAM.GrowTracking.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace ESAM.GrowTracking.Application.Services
{
    public class CurrentSessionIntegrityValidationService : ICurrentSessionIntegrityValidationService
    {
        private readonly ILogger<CurrentSessionIntegrityValidationService> _logger;
        private readonly IUserRepository _userRepository;
        private readonly IUserDeviceRepository _userDeviceRepository;
        private readonly IUserWorkProfileRepository _userWorkProfileRepository;
        private readonly IWorkProfileRepository _workProfileRepository;
        private readonly IWorkProfilePermissionRepository _workProfilePermissionRepository;
        private readonly IUserRoleCampusRepository _userRoleCampusRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly ICampusRepository _campusRepository;
        private readonly IRolePermissionRepository _rolePermissionRepository;

        public CurrentSessionIntegrityValidationService(ILogger<CurrentSessionIntegrityValidationService> logger, IUserRepository userRepository, 
            IUserDeviceRepository userDeviceRepository, IUserWorkProfileRepository userWorkProfileRepository, IWorkProfileRepository workProfileRepository, 
            IWorkProfilePermissionRepository workProfilePermissionRepository, IUserRoleCampusRepository userRoleCampusRepository, IRoleRepository roleRepository, 
            ICampusRepository campusRepository, IRolePermissionRepository rolePermissionRepository)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(userRepository);
            ArgumentNullException.ThrowIfNull(userDeviceRepository);
            ArgumentNullException.ThrowIfNull(userWorkProfileRepository);
            ArgumentNullException.ThrowIfNull(workProfileRepository);
            ArgumentNullException.ThrowIfNull(workProfilePermissionRepository);
            ArgumentNullException.ThrowIfNull(userRoleCampusRepository);
            ArgumentNullException.ThrowIfNull(roleRepository);
            ArgumentNullException.ThrowIfNull(campusRepository);
            ArgumentNullException.ThrowIfNull(rolePermissionRepository);
            _logger = logger;
            _userRepository = userRepository;
            _userDeviceRepository = userDeviceRepository;
            _userWorkProfileRepository = userWorkProfileRepository;
            _workProfileRepository = workProfileRepository;
            _workProfilePermissionRepository = workProfilePermissionRepository;
            _userRoleCampusRepository = userRoleCampusRepository;
            _roleRepository = roleRepository;
            _campusRepository = campusRepository;
            _rolePermissionRepository = rolePermissionRepository;
        }

        public async Task<Result> ValidateUserContextAsync(int currentUserId, string currentSecurityStamp, int currentTokenVersion, DateTime utcNow, bool asTracking = false, 
            CancellationToken cancellationToken = default)
        {
            var isUserActiveAndUnlocked = await _userRepository.IsActiveAndUnlockedAsync(currentUserId, utcNow, asTracking, cancellationToken);
            if (!isUserActiveAndUnlocked)
            {
                _logger.LogWarning("ValidateUserContextAsync: usuario inválido o bloqueado. UserId={UserId}", currentUserId);
                return Result.Fail(Error.Unauthorized("Usuario inválido o bloqueado."));
            }
            var useHasValidSecurityCredentials = await _userRepository.HasValidSecurityCredentialsAsync(currentUserId, currentSecurityStamp, currentTokenVersion, asTracking,
                cancellationToken);
            if (!useHasValidSecurityCredentials)
            {
                _logger.LogWarning("ValidateUserContextAsync: usuario inválido por cambios en la cuenta. UserId={UserId}", currentUserId);
                return Result.Fail(Error.Unauthorized("Usuario inválido por cambios en la cuenta."));
            }
            return Result.Ok();
        }

        public async Task<Result> ValidateUserDeviceAsync(int currentUserDeviceId, int currentUserId, DateTime utcNow, bool asTracking = false, 
            CancellationToken cancellationToken = default)
        {
            var isUserDeviceActiveAndUnlocked = await _userDeviceRepository.IsActiveAndUnlockedAsync(currentUserDeviceId, currentUserId, utcNow, asTracking, cancellationToken);
            if (!isUserDeviceActiveAndUnlocked)
            {
                _logger.LogWarning("ValidateUserDeviceAsync: dispositivo inválido o bloqueado. UserId={UserId}, DeviceId={DeviceId}", currentUserId, currentUserDeviceId);
                return Result.Fail(Error.Unauthorized("Dispositivo inválido o bloqueado."));
            }
            return Result.Ok();
        }

        public async Task<Result> ValidateWorkProfileContextAsync(int currentUserId, int currentWorkProfileId, WorkProfileType workProfileType, bool asTracking = false, 
            CancellationToken cancellationToken = default)
        {
            var isUserWorkProfileActive = await _userWorkProfileRepository.IsActiveAsync(currentUserId, currentWorkProfileId, asTracking, cancellationToken);
            if (!isUserWorkProfileActive)
            {
                _logger.LogWarning("ValidateWorkProfileContextAsync: perfil de trabajo de usuario no encontrado o eliminado. UserId={UserId}, WorkProfileId={WorkProfileId}", 
                    currentUserId, currentWorkProfileId);
                return Result.Fail(Error.NotFound("No se encontró un perfil de trabajo activo asignado al usuario."));
            }
            var isWorkProfileActiveAndOfType = await _workProfileRepository.IsActiveAndOfTypeAsync(currentWorkProfileId, workProfileType, asTracking, cancellationToken);
            if (!isWorkProfileActiveAndOfType)
            {
                _logger.LogWarning("ValidateWorkProfileContextAsync: tipo de perfil de trabajo del usuario inválido. WorkProfileId={WorkProfileId}, TipoEsperado={ExpectedType}", 
                    currentWorkProfileId, workProfileType);
                return Result.Fail(Error.BusinessRule("El perfil de trabajo no corresponde al tipo esperado."));
            }
            return Result.Ok();
        }

        public async Task<Result> ValidateWorkProfileContextAndPermissionsAsync(int currentUserId, int currentWorkProfileId, WorkProfileType workProfileType, 
            bool asTracking = false, CancellationToken cancellationToken = default)
        {
            var validateWorkProfileContextResult = await ValidateWorkProfileContextAsync(currentUserId, currentWorkProfileId, workProfileType, asTracking, cancellationToken);
            if (validateWorkProfileContextResult.IsFailure)
                return Result.Fail(validateWorkProfileContextResult.Errors);
            var workProfileHasActivePermissions = await _workProfilePermissionRepository.HasActivePermissionsAsync(currentWorkProfileId, asTracking, cancellationToken);
            if (!workProfileHasActivePermissions)
            {
                _logger.LogWarning("ValidateWorkProfileContextAndPermissionsAsync: perfil de trabajo sin permisos activos. WorkProfileId={WorkProfileId}", 
                    currentWorkProfileId);
                return Result.Fail(Error.Forbidden("El perfil de trabajo no tiene permisos activos asignados."));
            }
            return Result.Ok();
        }
        
        public async Task<Result> ValidateCampusRoleContextAndPermissionsAsync(int currentUserId, int currentRoleId, int currentCampusId, bool asTracking = false,
            CancellationToken cancellationToken = default)
        {
            var isUserRoleCampusActive = await _userRoleCampusRepository.IsActiveAsync(currentUserId, currentRoleId, currentCampusId, asTracking, cancellationToken);
            if (!isUserRoleCampusActive)
            {
                _logger.LogWarning("ValidateCampusRoleContextAndPermissionsAsync: rol de sede no encontrado o eliminado. UserId={UserId}, RoleId={RoleId}, CampusId={CampusId}",
                    currentUserId, currentRoleId, currentCampusId);
                return Result.Fail(Error.NotFound("No se encontró un rol de sede activo asignado al usuario."));
            }
            var isRoleActive = await _roleRepository.IsActiveAsync(currentRoleId, asTracking, cancellationToken);
            if (!isRoleActive)
            {
                _logger.LogWarning("ValidateCampusRoleContextAndPermissionsAsync: rol no encontrado o eliminado. RoleId={RoleId}", currentRoleId);
                return Result.Fail(Error.BusinessRule("Rol no encontrado o eliminado."));
            }
            var isCampusActive = await _campusRepository.IsActiveAsync(currentCampusId, asTracking, cancellationToken);
            if (!isCampusActive)
            {
                _logger.LogWarning("ValidateCampusRoleContextAndPermissionsAsync: sede no encontrado o eliminado. CampusId={CampusId}", currentRoleId);
                return Result.Fail(Error.BusinessRule("Sede no encontrado o eliminado."));
            }
            var roleHasActivePermissions = await _rolePermissionRepository.HasActivePermissionsAsync(currentRoleId, asTracking, cancellationToken);
            if (!roleHasActivePermissions)
            {
                _logger.LogWarning("ValidateCampusRoleContextAndPermissionsAsync: el rol no tiene permisos activos. RoleId={RoleId}", currentRoleId);
                return Result.Fail(Error.Forbidden("El rol no tiene permisos activos asignados."));
            }
            return Result.Ok();
        }
    }
}