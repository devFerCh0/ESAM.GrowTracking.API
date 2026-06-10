using ESAM.GrowTracking.Application.Abstractions.Services;
using ESAM.GrowTracking.Application.Results;
using ESAM.GrowTracking.Application.ValueObjects;
using ESAM.GrowTracking.Domain.Abstractions.DataAccess.Repositories;
using Microsoft.Extensions.Logging;

namespace ESAM.GrowTracking.Application.Services
{
    public class CurrentSessionIntegrityValidationService : ICurrentSessionIntegrityValidationService
    {
        private readonly ILogger<CurrentSessionIntegrityValidationService> _logger;
        private readonly IUserRepository _userRepository;
        private readonly IUserDeviceRepository _userDeviceRepository;

        public CurrentSessionIntegrityValidationService(ILogger<CurrentSessionIntegrityValidationService> logger, IUserRepository userRepository, 
            IUserDeviceRepository userDeviceRepository)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(userRepository);
            ArgumentNullException.ThrowIfNull(userDeviceRepository);
            _logger = logger;
            _userRepository = userRepository;
            _userDeviceRepository = userDeviceRepository;
        }

        public async Task<Result> ValidateCurrentUserStatusAsync(int currentUserId, DateTime utcNow, bool asTracking = false, CancellationToken cancellationToken = default)
        {
            var validateCurrentUserStatus = await _userRepository.ValidateCurrentUserStatusAsync(currentUserId, utcNow, asTracking, cancellationToken);
            if (!validateCurrentUserStatus)
            {
                _logger.LogWarning("ValidateCurrentUserStatusAsync: usuario inválido o bloqueado. UserId={UserId}", currentUserId);
                return Result.Fail(Error.Unauthorized("Usuario inválido o bloqueado."));
            }
            return Result.Ok();
        }

        public async Task<Result> ValidateCurrentUserSecurityAsync(int currentUserId, string currentSecurityStamp, int currentTokenVersion, bool asTracking = false, 
            CancellationToken cancellationToken = default)
        {
            var validateCurrentUserSecurity = await _userRepository.ValidateCurrentUserSecurityAsync(currentUserId, currentSecurityStamp, currentTokenVersion, asTracking, 
                cancellationToken);
            if (!validateCurrentUserSecurity)
            {
                _logger.LogWarning("ValidateCurrentUserSecurityAsync: usuario inválido por cambios en la cuenta. UserId={UserId}", currentUserId);
                return Result.Fail(Error.Unauthorized("Usuario inválido por cambios en la cuenta."));
            }
            return Result.Ok();
        }

        public async Task<Result> ValidateCurrentUserAsync(int currentUserId, string currentSecurityStamp, int currentTokenVersion, DateTime utcNow, bool asTracking = false, 
            CancellationToken cancellationToken = default)
        {
            var validateCurrentUserStatus = await ValidateCurrentUserStatusAsync(currentUserId, utcNow, asTracking, cancellationToken);
            if (validateCurrentUserStatus.IsFailure)
                return Result.Fail(validateCurrentUserStatus.Errors);
            var validateCurrentUserSecurity = await ValidateCurrentUserSecurityAsync(currentUserId, currentSecurityStamp, currentTokenVersion, asTracking, cancellationToken);
            if (validateCurrentUserSecurity.IsFailure)
                return Result.Fail(validateCurrentUserSecurity.Errors);
            return Result.Ok();
        }

        public async Task<Result> ValidateCurrentUserDeviceStatusAsync(int currentUserDeviceId, int currentUserId, DateTime utcNow, bool asTracking = false, 
            CancellationToken cancellationToken = default)
        {
            var validateCurrentUserDeviceStatus = await _userDeviceRepository.ValidateCurrentUserDeviceStatusAsync(currentUserDeviceId, currentUserId, utcNow, asTracking, 
                cancellationToken);
            if (!validateCurrentUserDeviceStatus)
            {
                _logger.LogWarning("ValidateCurrentUserDeviceStatusAsync: dispositivo inválido o bloqueado. UserId={UserId}, DeviceId={DeviceId}", currentUserId, 
                    currentUserDeviceId);
                return Result.Fail(Error.Unauthorized("Dispositivo inválido o bloqueado."));
            }
            return Result.Ok();
        }
    }
}