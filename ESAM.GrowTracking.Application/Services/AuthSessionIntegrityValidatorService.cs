using ESAM.GrowTracking.Application.Abstractions.Services;
using ESAM.GrowTracking.Application.Abstractions.Services.Results;
using ESAM.GrowTracking.Application.Helpers;
using ESAM.GrowTracking.Application.ValueObjects;
using ESAM.GrowTracking.Domain.Abstractions.DataAccess.Repositories;
using ESAM.GrowTracking.Domain.Entities;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace ESAM.GrowTracking.Application.Services
{
    public sealed class AuthSessionIntegrityValidatorService : IAuthSessionIntegrityValidatorService
    {
        private readonly ILogger<AuthSessionIntegrityValidatorService> _logger;
        private readonly IUserSessionRefreshTokenRepository _userSessionRefreshTokenRepository;
        private readonly IUserSessionRepository _userSessionRepository;
        private readonly IUserSessionService _userSessionService;
        private readonly IHashService _hashService;

        public AuthSessionIntegrityValidatorService(ILogger<AuthSessionIntegrityValidatorService> logger, IUserSessionRefreshTokenRepository userSessionRefreshTokenRepository, 
            IUserSessionRepository userSessionRepository, IUserSessionService userSessionService, IHashService hashService)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(userSessionRefreshTokenRepository);
            ArgumentNullException.ThrowIfNull(userSessionRepository);
            ArgumentNullException.ThrowIfNull(userSessionService);
            ArgumentNullException.ThrowIfNull(hashService);
            _logger = logger;
            _userSessionRefreshTokenRepository = userSessionRefreshTokenRepository;
            _userSessionRepository = userSessionRepository;
            _userSessionService = userSessionService;
            _hashService = hashService;
        }

        public async Task<AuthSessionIntegrityResult> ValidateTemporaryAccessTokenAsync(string operationContext, string? refreshTokenRaw, string currentJti, 
            DateTime currentAccessTokenExpiration, int currentUserId, DateTime utcNow, bool asTracking, CancellationToken cancellationToken)
        {
            if (refreshTokenRaw is null)
            {
                var noRtReason = $"{operationContext}: Cierre/renovación de Token Temporal sin Refresh Token asociado.";
                _logger.LogInformation("{Reason} JTI: {Jti}, UserId: {UserId}.", noRtReason, currentJti, currentUserId);
                await RevokeTemporaryScopeAsync(null, noRtReason, currentUserId, currentJti, currentAccessTokenExpiration, utcNow, asTracking, cancellationToken);
                return AuthSessionIntegrityResult.Invalid(Error.Validation("Los tokens temporales no admiten renovación."));
            }
            RefreshTokenParser.TryParse(refreshTokenRaw, out var identifier, out _);
            if (identifier is null)
            {
                var malformedReason = $"{operationContext}: Uso de RT malformado con Token Temporal.";
                _logger.LogWarning("{Reason} JTI: {Jti}, UserId: {UserId}.", malformedReason, currentJti, currentUserId);
                await RevokeTemporaryScopeAsync(null, malformedReason, currentUserId, currentJti, currentAccessTokenExpiration, utcNow, asTracking, cancellationToken);
                return AuthSessionIntegrityResult.Invalid(Error.Validation("Formato de token inválido."));
            }
            var userSessionRefreshToken = await _userSessionRefreshTokenRepository.GetByIdentifierAsync(identifier, asTracking, cancellationToken);
            if (userSessionRefreshToken is null)
            {
                var notRegisteredReason = $"{operationContext}: Uso de RT no registrado con Token Temporal.";
                _logger.LogWarning("{Reason} JTI: {Jti}, UserId: {UserId}.", notRegisteredReason, currentJti, currentUserId);
                await RevokeTemporaryScopeAsync(null, notRegisteredReason, currentUserId, currentJti, currentAccessTokenExpiration, utcNow, asTracking, cancellationToken);
                return AuthSessionIntegrityResult.Invalid(Error.Forbidden("Renovación denegada. Credenciales no registradas."));
            }
            var associatedUserSession = await _userSessionRepository.GetByIdAsync(userSessionRefreshToken.UserSessionId, asTracking, cancellationToken);
            if (associatedUserSession is not null)
            {
                var anomalousReason = $"{operationContext}: Uso anómalo de RT con Token Temporal.";
                _logger.LogWarning("{Reason} SessionId: {SessionId}, JTI: {Jti}, UserId: {UserId}.", anomalousReason, associatedUserSession.Id, currentJti, currentUserId);
                await RevokeTemporaryScopeAsync(associatedUserSession, anomalousReason, currentUserId, currentJti, currentAccessTokenExpiration, utcNow, asTracking, 
                    cancellationToken);
                return AuthSessionIntegrityResult.Invalid(Error.Forbidden("Renovación no permitida para tokens temporales. Sesión comprometida."));
            }
            var orphanReason = $"{operationContext}: Uso de RT huérfano con Token Temporal.";
            _logger.LogWarning("{Reason} JTI: {Jti}, UserId: {UserId}.", orphanReason, currentJti, currentUserId);
            await RevokeTemporaryScopeAsync(null, orphanReason, currentUserId, currentJti, currentAccessTokenExpiration, utcNow, asTracking, cancellationToken);
            return AuthSessionIntegrityResult.Invalid(Error.Forbidden("Renovación denegada. Token temporal inválido."));
        }

        public async Task<AuthSessionIntegrityResult> ValidateSessionAccessTokenAsync(string operationContext, string? refreshTokenRaw, string currentJti,
            DateTime currentAccessTokenExpiration, int currentUserId, int currentUserSessionId, int currentUserDeviceId, DateTime utcNow, bool asTracking,
            CancellationToken cancellationToken)
        {
            if (refreshTokenRaw is null)
                return await RevokeByCurrentSessionLookupAsync(currentUserSessionId, $"{operationContext}: Petición sin Refresh Token.",
                    Error.Validation("Se requiere un Refresh Token para esta operación."), currentUserId, currentJti, currentAccessTokenExpiration, utcNow, asTracking, 
                    cancellationToken);
            RefreshTokenParser.TryParse(refreshTokenRaw, out var identifier, out var tokenPlain);
            if (identifier is null)
                return await RevokeByCurrentSessionLookupAsync(currentUserSessionId, $"{operationContext}: Refresh Token malformado.",
                    Error.Validation("Formato de token erróneo."), currentUserId, currentJti, currentAccessTokenExpiration, utcNow, asTracking, cancellationToken);
            var userSessionRefreshToken = await _userSessionRefreshTokenRepository.GetByIdentifierAsync(identifier, asTracking, cancellationToken);
            if (userSessionRefreshToken is null)
                return await RevokeByCurrentSessionLookupAsync(currentUserSessionId, $"{operationContext}: Identificador de Refresh Token no registrado.",
                    Error.Unauthorized("Token no reconocido por el sistema."), currentUserId, currentJti, currentAccessTokenExpiration, utcNow, asTracking, cancellationToken);
            if (tokenPlain is null)
                return await ResolveRefreshTokenConflictAsync(currentUserSessionId, userSessionRefreshToken, $"{operationContext}: Payload de Refresh Token ausente.",
                    Error.Validation("Token incompleto proporcionado."), currentUserId, currentJti, currentAccessTokenExpiration, utcNow, asTracking, cancellationToken);
            var computedHash = _hashService.ComputeHash(tokenPlain, userSessionRefreshToken.Salt);
            var isHashValid = CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(computedHash), Encoding.UTF8.GetBytes(userSessionRefreshToken.TokenHash));
            if (!isHashValid)
                return await ResolveRefreshTokenConflictAsync(currentUserSessionId, userSessionRefreshToken,
                    $"{operationContext}: Discrepancia criptográfica (Fallo de Hash).", Error.Unauthorized("Credenciales de sesión inválidas."), currentUserId, currentJti, 
                    currentAccessTokenExpiration, utcNow, asTracking, cancellationToken);
            if (userSessionRefreshToken.UserSessionId != currentUserSessionId)
                return await ResolveRefreshTokenConflictAsync(currentUserSessionId, userSessionRefreshToken,
                    $"{operationContext}: RT criptográficamente válido pero vinculado a otra sesión (posible robo/reutilización cruzada).",
                    Error.Forbidden("Detectada anomalía de seguridad. Sesión invalidada."), currentUserId, currentJti, currentAccessTokenExpiration, utcNow, asTracking, 
                    cancellationToken);
            //var userSession = await _userSessionRepository.GetByIdAndUserIdAndUserDeviceIdAsync(currentUserSessionId, currentUserId, currentUserDeviceId, asTracking, 
            //    cancellationToken);
            //if (userSession is null)
            //{
            //    var mismatchReason = $"{operationContext}: Datos de dispositivo o sesión no coinciden.";
            //    _logger.LogWarning("{Reason} SessionId: {SessionId}, DeviceId: {DeviceId}, JTI: {Jti}, UserId: {UserId}.", mismatchReason, currentUserSessionId, 
            //        currentUserDeviceId, currentJti, currentUserId);
            //    await RevokeSessionScopeAsync(null, null, mismatchReason, currentUserId, currentUserSessionId, currentJti, currentAccessTokenExpiration, utcNow, asTracking, 
            //        cancellationToken);
            //    return AuthSessionIntegrityResult.Invalid(Error.Unauthorized("Sesión no válida o dispositivo no reconocido."));
            //}
            var userSession = await _userSessionRepository.GetByIdAndUserIdAndUserDeviceIdAsync(currentUserSessionId, currentUserId, currentUserDeviceId, asTracking,
                cancellationToken);
            if (userSession is null)
            {
                var mismatchReason = $"{operationContext}: Datos de dispositivo o sesión no coinciden.";
                _logger.LogWarning("{Reason} SessionId: {SessionId}, DeviceId: {DeviceId}, JTI: {Jti}, UserId: {UserId}.", mismatchReason, currentUserSessionId,
                    currentUserDeviceId, currentJti, currentUserId);
                var mismatchedSession = await _userSessionRepository.GetByIdAsync(currentUserSessionId, asTracking, cancellationToken);
                await RevokeSessionScopeAsync(mismatchedSession, null, mismatchReason, currentUserId, currentUserSessionId, currentJti, currentAccessTokenExpiration, utcNow,
                    asTracking, cancellationToken);
                return AuthSessionIntegrityResult.Invalid(Error.Unauthorized("Sesión no válida o dispositivo no reconocido."));
            }
            if (userSession.AbsoluteExpiresAt <= utcNow)
                return await RevokeSessionAndFailAsync(userSession, $"{operationContext}: Expiración absoluta alcanzada.",
                    Error.Unauthorized("La sesión ha expirado permanentemente."), currentUserId, currentUserSessionId, currentJti, currentAccessTokenExpiration, utcNow, asTracking, 
                    cancellationToken);
            if (userSession.ExpiresAt <= utcNow || userSessionRefreshToken.ExpiresAt <= utcNow)
                return await RevokeSessionAndFailAsync(userSession, $"{operationContext}: Tiempo de inactividad superado.",
                    Error.Unauthorized("La sesión por inactividad ha expirado."), currentUserId, currentUserSessionId, currentJti, currentAccessTokenExpiration, utcNow, asTracking, 
                    cancellationToken);
            if (userSessionRefreshToken.ReplacedByUserSessionRefreshTokenId.HasValue)
                return await RevokeSessionAndFailAsync(userSession, $"{operationContext}: Posible ataque de repetición. RT reemplazado.",
                    Error.Forbidden("Detectada anomalía de seguridad. Sesión invalidada."), currentUserId, currentUserSessionId, currentJti, currentAccessTokenExpiration, utcNow, 
                    asTracking, cancellationToken);
            if (userSession.IsRevoked || userSessionRefreshToken.IsRevoked)
                return await RevokeSessionAndFailAsync(userSession, $"{operationContext}: Uso de credenciales previamente revocadas.",
                    Error.Unauthorized("La sesión ya fue terminada previamente."), currentUserId, currentUserSessionId, currentJti, currentAccessTokenExpiration, utcNow, 
                    asTracking, cancellationToken);
            _logger.LogInformation("{Operation}: Validación de integridad exitosa para sesión {SessionId}. JTI: {Jti}, UserId: {UserId}.", operationContext, currentUserSessionId, 
                currentJti, currentUserId);
            return AuthSessionIntegrityResult.Success(userSession, userSessionRefreshToken);
        }

        private async Task<AuthSessionIntegrityResult> ResolveRefreshTokenConflictAsync(int currentUserSessionId, UserSessionRefreshToken userSessionRefreshToken, string reason, 
            Error error, int currentUserId, string currentJti, DateTime currentAccessTokenExpiration, DateTime utcNow, bool asTracking, CancellationToken cancellationToken)
        {
            if (userSessionRefreshToken.UserSessionId == currentUserSessionId)
                return await RevokeByCurrentSessionLookupAsync(currentUserSessionId, reason, error, currentUserId, currentJti, currentAccessTokenExpiration, utcNow,
                    asTracking, cancellationToken);
            var rtSession = await _userSessionRepository.GetByIdAsync(userSessionRefreshToken.UserSessionId, asTracking, cancellationToken);
            var atSession = await _userSessionRepository.GetByIdAsync(currentUserSessionId, asTracking, cancellationToken);
            _logger.LogWarning("{Reason} Conflicto cruzado. AT-SessionId: {AtSessionId}, RT-SessionId: {RtSessionId}, JTI: {Jti}, UserId: {UserId}.", reason, currentUserSessionId, 
                userSessionRefreshToken.UserSessionId, currentJti, currentUserId);
            await RevokeSessionScopeAsync(rtSession, atSession, reason, currentUserId, currentUserSessionId, currentJti, currentAccessTokenExpiration, utcNow, asTracking, 
                cancellationToken);
            return AuthSessionIntegrityResult.Invalid(error);
        }

        private async Task<AuthSessionIntegrityResult> RevokeByCurrentSessionLookupAsync(int currentUserSessionId, string reason, Error error, int currentUserId, string currentJti, 
            DateTime currentAccessTokenExpiration, DateTime utcNow, bool asTracking, CancellationToken cancellationToken)
        {
            var currentSession = await _userSessionRepository.GetByIdAsync(currentUserSessionId, asTracking, cancellationToken);
            _logger.LogWarning("{Reason} SessionId: {SessionId}, JTI: {Jti}, UserId: {UserId}.", reason, currentUserSessionId, currentJti, currentUserId);
            await RevokeSessionScopeAsync(currentSession, null, reason, currentUserId, currentUserSessionId, currentJti, currentAccessTokenExpiration, utcNow, asTracking, 
                cancellationToken);
            return AuthSessionIntegrityResult.Invalid(error);
        }

        private async Task<AuthSessionIntegrityResult> RevokeSessionAndFailAsync(UserSession userSession, string reason, Error error, int currentUserId, int currentUserSessionId, 
            string currentJti, DateTime currentAccessTokenExpiration, DateTime utcNow, bool asTracking, CancellationToken cancellationToken)
        {
            _logger.LogWarning("{Reason} SessionId: {SessionId}, JTI: {Jti}, UserId: {UserId}.", reason, currentUserSessionId, currentJti, currentUserId);
            await _userSessionService.RevokeUserSessionAndAccessTokenSessionAsync(userSession, currentJti, currentAccessTokenExpiration, reason, currentUserId, 
                currentUserSessionId, utcNow, asTracking, cancellationToken);
            return AuthSessionIntegrityResult.Invalid(error);
        }

        private Task RevokeTemporaryScopeAsync(UserSession? associatedUserSession, string reason, int currentUserId, string currentJti, DateTime currentAccessTokenExpiration, 
            DateTime utcNow, bool asTracking, CancellationToken cancellationToken)
        {
            return associatedUserSession is not null
                ? _userSessionService.RevokeUserSessionAndAccessTokenTemporaryAsync(associatedUserSession, currentJti, currentAccessTokenExpiration, reason, currentUserId, utcNow, 
                    asTracking, cancellationToken)
                : _userSessionService.RevokeAccessTokenTemporaryAsync(currentUserId, currentJti, currentAccessTokenExpiration, reason, utcNow, asTracking,  cancellationToken);
        }

        private Task RevokeSessionScopeAsync(UserSession? primarySession, UserSession? secondarySession, string reason, int currentUserId, int currentUserSessionId, 
            string currentJti, DateTime currentAccessTokenExpiration, DateTime utcNow, bool asTracking, CancellationToken cancellationToken)
        {
            if (primarySession is not null && secondarySession is not null)
                return _userSessionService.RevokeUserSessionAndAccessTokenSessionAsync(primarySession, secondarySession, currentJti, currentAccessTokenExpiration,
                    reason, currentUserId, currentUserSessionId, utcNow, asTracking, cancellationToken);
            var singleSession = primarySession ?? secondarySession;
            return singleSession is not null
                ? _userSessionService.RevokeUserSessionAndAccessTokenSessionAsync(singleSession, currentJti, currentAccessTokenExpiration, reason, currentUserId,
                    currentUserSessionId, utcNow, asTracking, cancellationToken)
                : _userSessionService.RevokeAccessTokenSessionAsync(currentUserSessionId, currentJti, currentAccessTokenExpiration, currentUserId, reason, utcNow, asTracking, 
                    cancellationToken);
        }
    }
}