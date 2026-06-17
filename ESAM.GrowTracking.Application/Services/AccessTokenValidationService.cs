using ESAM.GrowTracking.Application.Abstractions.Services;
using ESAM.GrowTracking.Application.Results;
using ESAM.GrowTracking.Application.ValueObjects;
using ESAM.GrowTracking.Domain.Abstractions.DataAccess.Repositories;
using ESAM.GrowTracking.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace ESAM.GrowTracking.Application.Services
{
    public class AccessTokenValidationService : IAccessTokenValidationService
    {
        private readonly ILogger<AccessTokenValidationService> _logger;
        private readonly IDateTimeService _dateTimeService;
        private readonly IUserRepository _userRepository;
        private readonly IUserDeviceRepository _userDeviceRepository;
        private readonly IBlacklistedAccessTokenTemporaryRepository _blacklistedAccessTokenTemporaryRepository;
        private readonly IBlacklistedAccessTokenSessionRepository _blacklistedAccessTokenSessionRepository;
        private readonly IUserSessionRepository _userSessionRepository;

        public AccessTokenValidationService(ILogger<AccessTokenValidationService> logger, IDateTimeService dateTimeService, IUserRepository userRepository, 
            IUserDeviceRepository userDeviceRepository, IBlacklistedAccessTokenTemporaryRepository blacklistedAccessTokenTemporaryRepository, 
            IBlacklistedAccessTokenSessionRepository blacklistedAccessTokenSessionRepository, IUserSessionRepository userSessionRepository)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(dateTimeService);
            ArgumentNullException.ThrowIfNull(userRepository);
            ArgumentNullException.ThrowIfNull(userDeviceRepository);
            ArgumentNullException.ThrowIfNull(blacklistedAccessTokenTemporaryRepository);
            ArgumentNullException.ThrowIfNull(blacklistedAccessTokenSessionRepository);
            ArgumentNullException.ThrowIfNull(userSessionRepository);
            _logger = logger;
            _dateTimeService = dateTimeService;
            _userRepository = userRepository;
            _userDeviceRepository = userDeviceRepository;
            _blacklistedAccessTokenTemporaryRepository = blacklistedAccessTokenTemporaryRepository;
            _blacklistedAccessTokenSessionRepository = blacklistedAccessTokenSessionRepository;
            _userSessionRepository = userSessionRepository;
        }

        public async Task<Result> ValidateCurrentTemporaryAsync(string currentJti, int currentUserId, string currentSecurityStamp, int currentTokenVersion, 
            int currentUserDeviceId, CancellationToken cancellationToken = default)
        {
            var asTracking = false;
            var utcNow = _dateTimeService.UtcNow;
            var doesBlacklistedAccessTokenTemporaryNotExist = await _blacklistedAccessTokenTemporaryRepository.DoesNotExistAsync(currentJti, asTracking, cancellationToken);
            if (!doesBlacklistedAccessTokenTemporaryNotExist)
            {
                _logger.LogWarning("ValidateCurrentTemporaryAsync: el jti del token temporal es inválido. Jti={Jti}", currentJti);
                return Result.Fail(Error.NotFound("El jti del token temporal es inválido."));
            }
            return await ValidateCurrentUserAndCurrentUserDeviceAsync(currentUserId, currentSecurityStamp, currentTokenVersion, currentUserDeviceId, utcNow, asTracking, 
                cancellationToken);
        }

        public async Task<Result> ValidateCurrentSessionAsync(string currentJti, int currentUserId, string currentSecurityStamp, int currentTokenVersion, int currentUserDeviceId,
            int currentUserSessionId, int currentWorkProfileId, WorkProfileType workProfileType, int currentRoleId, int currentCampusId, 
            CancellationToken cancellationToken = default)
        {
            var asTracking = false;
            var utcNow = _dateTimeService.UtcNow;
            var doesBlacklistedAccessTokenSessionNotExist = await _blacklistedAccessTokenSessionRepository.DoesNotExistAsync(currentJti, asTracking, cancellationToken);
            if (!doesBlacklistedAccessTokenSessionNotExist)
            {
                _logger.LogWarning("ValidateCurrentSessionAsync: el jti del token de sesión es inválido. Jti={Jti}", currentJti);
                return Result.Fail(Error.NotFound("El jti del token de sesión es inválido."));
            }
            var validateUserAndUserDevice = await ValidateCurrentUserAndCurrentUserDeviceAsync(currentUserId, currentSecurityStamp, currentTokenVersion, currentUserDeviceId, 
                utcNow, asTracking, cancellationToken);
            if (validateUserAndUserDevice.IsFailure)
                return validateUserAndUserDevice;
            var isUserSessionUnRevokedAndUnExpired = await _userSessionRepository.IsUnRevokedAndUnExpiredAsync(currentUserSessionId, currentUserId, utcNow, asTracking,
                cancellationToken);
            if (!isUserSessionUnRevokedAndUnExpired)
            {
                _logger.LogWarning("ValidateCurrentSessionAsync: sesión de usuario revocada o caducada. UserId={UserId}, UserSessionId={userSessionId}", currentUserId,
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

        private async Task<Result> ValidateCurrentUserAndCurrentUserDeviceAsync(int currentUserId, string currentSecurityStamp, int currentTokenVersion, int currentUserDeviceId, 
            DateTime utcNow, bool asTracking, CancellationToken cancellationToken)
        {
            var isUserActiveAndUnlocked = await _userRepository.IsActiveAndUnlockedAsync(currentUserId, utcNow, asTracking, cancellationToken);
            if (!isUserActiveAndUnlocked)
            {
                _logger.LogWarning("ValidateCurrentUserAndCurrentUserDeviceAsync: usuario inválido o bloqueado. UserId={UserId}", currentUserId);
                return Result.Fail(Error.Unauthorized("Usuario inválido o bloqueado."));
            }
            var userHasValidSecurityCredentials = await _userRepository.HasValidSecurityCredentialsAsync(currentUserId, currentSecurityStamp, currentTokenVersion, asTracking,
                cancellationToken);
            if (!userHasValidSecurityCredentials)
            {
                _logger.LogWarning("ValidateCurrentUserAndCurrentUserDeviceAsync: usuario inválido por cambios en la cuenta. UserId={UserId}", currentUserId);
                return Result.Fail(Error.Unauthorized("Usuario inválido por cambios en la cuenta."));
            }
            var isUserDeviceActiveAndUnlocked = await _userDeviceRepository.IsActiveAndUnlockedAsync(currentUserDeviceId, currentUserId, utcNow, asTracking, cancellationToken);
            if (!isUserDeviceActiveAndUnlocked)
            {
                _logger.LogWarning("ValidateCurrentUserAndCurrentUserDeviceAsync: dispositivo inválido o bloqueado. UserId={UserId}, DeviceId={DeviceId}", currentUserId, 
                    currentUserDeviceId);
                return Result.Fail(Error.Unauthorized("Dispositivo inválido o bloqueado."));
            }
            return Result.Ok();
        }
    }
}