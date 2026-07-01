using ESAM.GrowTracking.Application.Abstractions.Services;
using ESAM.GrowTracking.Application.Enums;
using ESAM.GrowTracking.Application.Extensions;
using ESAM.GrowTracking.Application.Helpers;
using ESAM.GrowTracking.Application.Results;
using ESAM.GrowTracking.Domain.Abstractions.DataAccess.Repositories;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace ESAM.GrowTracking.Application.Features.Auth.Logout
{
    public sealed class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result>
    {
        private readonly ILogger<LogoutCommandHandler> _logger;
        private readonly IValidator<LogoutCommand> _validator;
        private readonly IAccessTokenClaimsValidatorService _accessTokenClaimsValidatorService;
        private readonly IDateTimeService _dateTimeService;
        private readonly IUserSessionRefreshTokenRepository _userSessionRefreshTokenRepository;
        private readonly IUserSessionRepository _userSessionRepository;
        private readonly IUserSessionService _userSessionService;
        private readonly IHashService _hashService;

        public LogoutCommandHandler(ILogger<LogoutCommandHandler> logger, IValidator<LogoutCommand> validator, IAccessTokenClaimsValidatorService accessTokenClaimsValidatorService,
            IDateTimeService dateTimeService, IUserSessionRefreshTokenRepository userSessionRefreshTokenRepository, IUserSessionRepository userSessionRepository, 
            IUserSessionService userSessionService, IHashService hashService)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(validator);
            ArgumentNullException.ThrowIfNull(accessTokenClaimsValidatorService);
            ArgumentNullException.ThrowIfNull(dateTimeService);
            ArgumentNullException.ThrowIfNull(userSessionRefreshTokenRepository);
            ArgumentNullException.ThrowIfNull(userSessionRepository);
            ArgumentNullException.ThrowIfNull(userSessionService);
            ArgumentNullException.ThrowIfNull(hashService);
            _logger = logger;
            _validator = validator;
            _accessTokenClaimsValidatorService = accessTokenClaimsValidatorService;
            _dateTimeService = dateTimeService;
            _userSessionRefreshTokenRepository = userSessionRefreshTokenRepository;
            _userSessionRepository = userSessionRepository;
            _userSessionService = userSessionService;
            _hashService = hashService;
        }

        public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
        {
            var validation = await _validator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                _logger.LogWarning("LogoutCommand: Validación fallida. Errores: {Errors}", string.Join(" | ", validation.Errors.Select(e => e.ErrorMessage)));
                return Result.Fail(validation.ToCommandErrors());
            }
            var currentAccessTokenType = _accessTokenClaimsValidatorService.CurrentAccessTokenType;
            var asTracking = false;
            var currentUserId = _accessTokenClaimsValidatorService.CurrentUserId;
            var currentJti = _accessTokenClaimsValidatorService.CurrentJti;
            var currentAccessTokenExpiration = _accessTokenClaimsValidatorService.CurrentAccessTokenExpiration;
            var utcNow = _dateTimeService.UtcNow;
            if (currentAccessTokenType == AccessTokenType.Temporary)
                return await LogoutAccessTokenTemporary(request.RefreshTokenRaw, currentJti, currentAccessTokenExpiration, currentUserId, utcNow, asTracking, cancellationToken);
            else
            {
                var currentUserSessionId = _accessTokenClaimsValidatorService.CurrentUserSessionId;
                var currentUserDeviceId = _accessTokenClaimsValidatorService.CurrentUserDeviceId;
                return await LogoutAccessTokenSession(request.RefreshTokenRaw, currentJti, currentAccessTokenExpiration, currentUserId, currentUserSessionId, currentUserDeviceId, 
                    utcNow, asTracking, cancellationToken);
            }
        }

        // CUANDO EL CLAIM CURRENTACCESSTOKENTYPE DEL ACCESS TOKEN ES DE TIPO TEMPORARY: NO SE TIENE SESION DE USUARIO Y POR LO TANTO TAMPOCO SE TIENE REFRESH TOKEN.
        // SI SE TIENE REFRESH TOKEN ES ALGO ANOMALO POR LO TANTO HAY QUE HACER SEGUIMIENTO PARA REVOCARLOM, SI SE ENCUENTRA QUE EL REFRESH TOKEN TIENE UNA SESION,
        // SE REVOCA LA SESION POR ANOMALIA Y SE REVOCA EL ACCESSTOKEN TEMPORAL Y SE MANDA A LA LISTA NEGRA DE TOKENS TEMPORALES
        // EL ACCESS TOKEN SE REVOCA TENGA O NO TENGA REFRESH TOKEN Y SE VA SU LISTA NEGRA DE TOKEN TEMPORALES
        private async Task<Result> LogoutAccessTokenTemporary(string? refreshTokenRaw, string currentJti, DateTime currentAccessTokenExpiration, int currentUserId, DateTime utcNow, 
            bool asTracking = false, CancellationToken cancellationToken = default)
        {
            if (refreshTokenRaw is not null)
            {
                RefreshTokenParser.TryParse(refreshTokenRaw, out var identifier, out _);
                if (identifier is not null)
                {
                    var userSessionRefreshToken = await _userSessionRefreshTokenRepository.GetByIdentifierAsync(identifier, asTracking, cancellationToken);
                    if (userSessionRefreshToken is not null)
                    {
                        var mismatchedUserSession = await _userSessionRepository.GetByIdAsync(userSessionRefreshToken.UserSessionId, asTracking, cancellationToken);
                        if (mismatchedUserSession is not null)
                        {
                            _logger.LogWarning("Logout: Se recibió un Access Token Temporal (JTI: {Jti}) " + 
                                "junto con un Refresh Token válido asociado a la sesión {SessionId} del usuario {UserId}.", currentJti, mismatchedUserSession.Id, currentUserId);
                            await _userSessionService.RevokeUserSessionAndAccessTokenTemporaryAsync(mismatchedUserSession, currentJti, currentAccessTokenExpiration,
                                "Logout: Intento de uso de Refresh Token con Access Token Temporal. Compromiso de sesión detectado.", currentUserId, utcNow, asTracking, 
                                cancellationToken);
                            return Result.Ok();
                        }
                        else
                        {
                            _logger.LogWarning("Logout: Se recibió un Access Token Temporal (JTI: {Jti}) junto con un Refresh Token cuyo registro de sesión " + 
                                "({SessionId}) no existe en BD para el usuario {UserId}.", currentJti, userSessionRefreshToken.UserSessionId, currentUserId);
                            await _userSessionService.BlacklistedAccessTokenTemporaryAsync(currentUserId, currentJti, currentAccessTokenExpiration,
                                "Logout: Intento de uso de Refresh Token huérfano con Access Token Temporal.", utcNow, cancellationToken);
                            return Result.Ok();
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Logout: Se recibió un Access Token Temporal (JTI: {Jti}) junto con un identificador de Refresh Token " + 
                            "inexistente en BD para el usuario {UserId}.", currentJti, currentUserId);
                        await _userSessionService.BlacklistedAccessTokenTemporaryAsync(currentUserId, currentJti, currentAccessTokenExpiration,
                            "Logout: Intento de uso de Refresh Token no registrado con Access Token Temporal.", utcNow, cancellationToken);
                        return Result.Ok();
                    }
                }
                else
                {
                    _logger.LogWarning("Logout: Se recibió un Access Token Temporal (JTI: {Jti}) junto con un Refresh Token malformado para el usuario {UserId}.", currentJti, 
                        currentUserId);
                    await _userSessionService.BlacklistedAccessTokenTemporaryAsync(currentUserId, currentJti, currentAccessTokenExpiration,
                        "Logout: Intento de uso de Refresh Token malformado con Access Token Temporal.", utcNow, cancellationToken);
                    return Result.Ok();
                }
            }
            else
            {
                _logger.LogInformation("Logout: Access Token Temporal (JTI: {Jti}) revocado correctamente para el usuario {UserId}.", currentJti, currentUserId);
                await _userSessionService.BlacklistedAccessTokenTemporaryAsync(currentUserId, currentJti, currentAccessTokenExpiration,
                    "Logout: Cierre de sesión temporal exitoso.", utcNow, cancellationToken);
                return Result.Ok();
            }
        }

        // CUANDO EL CLAIM CURRENTACCESSTOKENTYPE DEL ACCESS TOKEN ES DE TIPO SESSION: SE TIENE SESION DE USUARIO Y POR LO TANTO SE TIENE REFRESH TOKEN.
        // EL ACCESS TOKEN SE REVOCA Y SE VA SU LISTA NEGRA DE TOKEN SESSION
        private async Task<Result> LogoutAccessTokenSession(string? refreshTokenRaw, string currentJti, DateTime currentAccessTokenExpiration, int currentUserId, 
            int currentUserSessionId, int currentUserDeviceId, DateTime utcNow, bool asTracking = false, CancellationToken cancellationToken = default)
        {
            if (refreshTokenRaw is not null)
            {
                RefreshTokenParser.TryParse(refreshTokenRaw, out var identifier, out var tokenPlain);
                if (identifier is not null)
                {
                    var userSessionRefreshToken = await _userSessionRefreshTokenRepository.GetByIdentifierAsync(identifier, asTracking, cancellationToken);
                    if (userSessionRefreshToken is not null)
                    {
                        if (tokenPlain is not null)
                        {
                            var computedHash = _hashService.ComputeHash(tokenPlain, userSessionRefreshToken.Salt);
                            if (CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(computedHash), Encoding.UTF8.GetBytes(userSessionRefreshToken.TokenHash)))
                            {
                                var userSession = await _userSessionRepository.GetByIdAndUserIdAndUserDeviceIdAsync(currentUserSessionId, currentUserId, currentUserDeviceId, 
                                    asTracking, cancellationToken);
                                if (userSession is not null)
                                {
                                    if (userSession.AbsoluteExpiresAt <= utcNow)
                                    {
                                        _logger.LogWarning("Logout: Intento de cierre de sesión válido pero la sesión {SessionId} ya alcanzó su expiración absoluta. JTI: {Jti}, " +
                                            "UserId: {UserId}.", currentUserSessionId, currentJti, currentUserId);
                                        await _userSessionService.RevokeUserSessionAndAccessTokenSessionAsync(userSession, currentJti, currentAccessTokenExpiration,
                                            "Logout: Intento de cierre sobre una sesión que ya superó su tiempo de expiración absoluta.", currentUserId, currentUserSessionId, 
                                            utcNow, asTracking, cancellationToken);
                                        return Result.Ok();
                                    }
                                    else if (userSession.ExpiresAt <= utcNow || userSessionRefreshToken.ExpiresAt <= utcNow)
                                    {
                                        _logger.LogWarning("Logout: Intento de cierre de sesión válido pero la sesión {SessionId} o el Refresh Token ya estaban expirados. " + 
                                            "JTI: {Jti}, UserId: {UserId}.", currentUserSessionId, currentJti, currentUserId);
                                        await _userSessionService.RevokeUserSessionAndAccessTokenSessionAsync(userSession, currentJti, currentAccessTokenExpiration,
                                            "Logout: Intento de cierre sobre una sesión o Refresh Token inactivos/expirados.", currentUserId, currentUserSessionId, utcNow, 
                                            asTracking, cancellationToken);
                                        return Result.Ok();
                                    }
                                    else if (userSession.IsRevoked || userSessionRefreshToken.IsRevoked)
                                    {
                                        _logger.LogWarning("Logout: Intento de cierre de sesión sobre la sesión {SessionId} o Refresh Token que ya se encontraban revocados. " + 
                                            "Posible uso de credenciales cacheadas o comprometidas. JTI: {Jti}, UserId: {UserId}.", currentUserSessionId, currentJti, 
                                            currentUserId);
                                        await _userSessionService.RevokeUserSessionAndAccessTokenSessionAsync(userSession, currentJti, currentAccessTokenExpiration,
                                            "Logout: Intento de cierre sobre una sesión o Refresh Token previamente revocados.", currentUserId, currentUserSessionId, utcNow, 
                                            asTracking, cancellationToken);
                                        return Result.Ok();
                                    }
                                    else if (userSessionRefreshToken.ReplacedByUserSessionRefreshTokenId.HasValue)
                                    {
                                        _logger.LogWarning("Logout: Intento de cierre de sesión utilizando un Refresh Token ya rotado en la sesión {SessionId}. " + 
                                            "Posible ataque de repetición (Replay Attack). JTI: {Jti}, UserId: {UserId}.", currentUserSessionId, currentJti, currentUserId);
                                        await _userSessionService.RevokeUserSessionAndAccessTokenSessionAsync(userSession, currentJti, currentAccessTokenExpiration,
                                            "Logout: Intento de uso de un Refresh Token que ya fue reemplazado (Posible ataque de repetición).", currentUserId, 
                                            currentUserSessionId, utcNow, asTracking, cancellationToken);
                                        return Result.Ok();
                                    }
                                    else
                                    {
                                        _logger.LogInformation("Logout: Sesión {SessionId} y Access Token (JTI: {Jti}) revocados correctamente para el usuario " +
                                            "{UserId}.", currentUserSessionId, currentJti, currentUserId);
                                        await _userSessionService.RevokeUserSessionAndAccessTokenSessionAsync(userSession, currentJti, currentAccessTokenExpiration,
                                            "Logout: Cierre de sesión exitoso.", currentUserId, currentUserSessionId, utcNow, asTracking, cancellationToken);
                                        return Result.Ok();
                                    }
                                }
                                else
                                {
                                    _logger.LogWarning("Logout: Credenciales válidas pero la sesión {SessionId} o el dispositivo {DeviceId} no coinciden/no existen para el " + 
                                        "usuario {UserId}. JTI: {Jti}.", currentUserSessionId, currentUserDeviceId, currentUserId, currentJti);
                                    await _userSessionService.BlacklistedAccessTokenSessionAsync(currentUserSessionId, currentJti, currentAccessTokenExpiration, currentUserId,
                                        "Logout: Sesión o dispositivo de usuario no encontrados durante un cierre de sesión válido.", utcNow, cancellationToken);
                                    return Result.Ok();
                                }
                            }
                            else
                            {
                                if (userSessionRefreshToken.UserSessionId == currentUserSessionId)
                                {
                                    var mismatchedUserSession = await _userSessionRepository.GetByIdAsync(currentUserSessionId, asTracking, cancellationToken);
                                    if (mismatchedUserSession is not null)
                                    {
                                        _logger.LogWarning("Logout: Fallo criptográfico en el Refresh Token para la sesión actual {SessionId} del usuario {UserId}. " + 
                                            "Posible intento de falsificación. JTI: {Jti}.", currentUserSessionId, currentUserId, currentJti);
                                        await _userSessionService.RevokeUserSessionAndAccessTokenSessionAsync(mismatchedUserSession, currentJti, currentAccessTokenExpiration,
                                            "Logout: Discrepancia criptográfica en el Refresh Token (Fallo de Hash).", currentUserId, currentUserSessionId, utcNow, asTracking, 
                                            cancellationToken);
                                        return Result.Ok();
                                    }
                                    else
                                    {
                                        _logger.LogWarning("Logout: Fallo criptográfico y sesión {SessionId} inexistente para el usuario {UserId}. JTI: {Jti}.", 
                                            currentUserSessionId, currentUserId, currentJti);
                                        await _userSessionService.BlacklistedAccessTokenSessionAsync(currentUserSessionId, currentJti, currentAccessTokenExpiration, currentUserId,
                                            "Logout: Discrepancia criptográfica y sesión no encontrada.", utcNow, cancellationToken);
                                        return Result.Ok();
                                    }
                                }
                                else
                                {
                                    var mismatchedUserSession = await _userSessionRepository.GetByIdAsync(userSessionRefreshToken.UserSessionId, asTracking, cancellationToken);
                                    var mismatchedUserSession1 = await _userSessionRepository.GetByIdAsync(currentUserSessionId, asTracking, cancellationToken);
                                    if (mismatchedUserSession is not null && mismatchedUserSession1 is not null)
                                    {
                                        _logger.LogWarning("Logout: Conflicto cruzado de sesiones. Hash inválido. Access Token reclama sesión {Session1}, pero Refresh Token " + 
                                            "apunta a sesión {Session2}. Ambas serán revocadas. JTI: {Jti}, UserId: {UserId}.", currentUserSessionId, 
                                            userSessionRefreshToken.UserSessionId, currentJti, currentUserId);
                                        await _userSessionService.RevokeUserSessionAndAccessTokenSessionAsync(mismatchedUserSession, mismatchedUserSession1, currentJti,
                                            currentAccessTokenExpiration, "Logout: Conflicto de sesión cruzada y fallo criptográfico detectado. Ambas sesiones " + 
                                            "comprometidas.", currentUserId, currentUserSessionId, utcNow, asTracking, cancellationToken);
                                        return Result.Ok();
                                    }
                                    else if (mismatchedUserSession is not null && mismatchedUserSession1 is null)
                                    {
                                        _logger.LogWarning("Logout: Conflicto cruzado. Access Token reclama sesión inexistente {Session1}, Refresh Token apunta a sesión " + 
                                            "existente {Session2} (Hash inválido). Revocando {Session2}. JTI: {Jti}, UserId: {UserId}.", currentUserSessionId, 
                                            userSessionRefreshToken.UserSessionId, userSessionRefreshToken.UserSessionId, currentJti, currentUserId);
                                        await _userSessionService.RevokeUserSessionAndAccessTokenSessionAsync(mismatchedUserSession, currentJti, currentAccessTokenExpiration,
                                            "Logout: Conflicto de sesión cruzada (Hash inválido). Access Token huérfano.", currentUserId, currentUserSessionId, utcNow, asTracking, 
                                            cancellationToken);
                                        return Result.Ok();
                                    }
                                    else if (mismatchedUserSession is null && mismatchedUserSession1 is not null)
                                    {

                                        _logger.LogWarning("Logout : Conflicto cruzado. Access Token reclama sesión existente {Session1}, Refresh Token apunta a sesión " +
                                            "inexistente {Session2} (Hash inválido). Revocando {Session1}. JTI: {Jti}, UserId: {UserId}.", currentUserSessionId,
                                            userSessionRefreshToken.UserSessionId, currentUserSessionId, currentJti, currentUserId); 
                                        await _userSessionService.RevokeUserSessionAndAccessTokenSessionAsync(mismatchedUserSession1, currentJti, currentAccessTokenExpiration,
                                            "Logout: Conflicto de sesión cruzada (Hash inválido). Refresh Token huérfano.", currentUserId, currentUserSessionId, utcNow, asTracking, 
                                            cancellationToken);
                                        return Result.Ok();
                                    }
                                    else
                                    {
                                        _logger.LogWarning("Logout: Conflicto cruzado total. Ni la sesión del Access Token ({Session1}) ni la del Refresh Token ({Session2}) " + 
                                            "existen. Fallo criptográfico. JTI: {Jti}, UserId: {UserId}.", currentUserSessionId, userSessionRefreshToken.UserSessionId, currentJti, 
                                            currentUserId);
                                        await _userSessionService.BlacklistedAccessTokenSessionAsync(currentUserSessionId, currentJti, currentAccessTokenExpiration, currentUserId,
                                            "Logout: Conflicto de sesión cruzada (Hash inválido) y ambas sesiones son inexistentes.", utcNow, cancellationToken);
                                        return Result.Ok();
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (userSessionRefreshToken.UserSessionId == currentUserSessionId)
                            {
                                var mismatchedUserSession = await _userSessionRepository.GetByIdAsync(currentUserSessionId, asTracking, cancellationToken);
                                if (mismatchedUserSession is not null)
                                {
                                    _logger.LogWarning("Logout: Refresh Token parseado parcialmente (sin tokenPlain) para la sesión {SessionId}. Posible manipulación de " + 
                                        "payload. JTI: {Jti}, UserId: {UserId}.", currentUserSessionId, currentJti, currentUserId);
                                    await _userSessionService.RevokeUserSessionAndAccessTokenSessionAsync(mismatchedUserSession, currentJti, currentAccessTokenExpiration,
                                        "Logout: Payload de Refresh Token ausente o malformado.", currentUserId, currentUserSessionId, utcNow, asTracking, cancellationToken);
                                    return Result.Ok();
                                }
                                else
                                {
                                    _logger.LogWarning("Logout: Refresh Token parseado parcialmente (sin tokenPlain) y sesión {SessionId} inexistente. JTI: {Jti}, " + 
                                        "UserId: {UserId}.", currentUserSessionId, currentJti, currentUserId);
                                    await _userSessionService.BlacklistedAccessTokenSessionAsync(currentUserSessionId, currentJti, currentAccessTokenExpiration, currentUserId,
                                        "Logout: Payload de Refresh Token ausente y sesión no encontrada.", utcNow, cancellationToken);
                                    return Result.Ok();
                                }
                            }
                            else
                            {
                                var mismatchedUserSession = await _userSessionRepository.GetByIdAsync(userSessionRefreshToken.UserSessionId, asTracking, cancellationToken);
                                var mismatchedUserSession1 = await _userSessionRepository.GetByIdAsync(currentUserSessionId, asTracking, cancellationToken);
                                if (mismatchedUserSession is not null && mismatchedUserSession1 is not null)
                                {
                                    _logger.LogWarning("Logout: Conflicto cruzado de sesiones. Sin tokenPlain. Access Token reclama {Session1}, Refresh Token apunta a " + 
                                        "{Session2}. JTI: {Jti}, UserId: {UserId}.", currentUserSessionId, userSessionRefreshToken.UserSessionId, currentJti, currentUserId);
                                    await _userSessionService.RevokeUserSessionAndAccessTokenSessionAsync(mismatchedUserSession, mismatchedUserSession1, currentJti, 
                                        currentAccessTokenExpiration, "Logout: Conflicto de sesión cruzada (Sin Payload RT). Ambas sesiones comprometidas.", currentUserId, 
                                        currentUserSessionId, utcNow, asTracking, cancellationToken);
                                    return Result.Ok();
                                }
                                else if (mismatchedUserSession is not null && mismatchedUserSession1 is null)
                                {
                                    _logger.LogWarning("Logout: Conflicto cruzado sin tokenPlain. Sesión de Access Token ({Session1}) inexistente. Revocando sesión de RT " + 
                                        "({Session2}). JTI: {Jti}, UserId: {UserId}.", currentUserSessionId, userSessionRefreshToken.UserSessionId, currentJti, currentUserId);
                                    await _userSessionService.RevokeUserSessionAndAccessTokenSessionAsync(mismatchedUserSession, currentJti, currentAccessTokenExpiration,
                                        "Logout: Conflicto de sesión cruzada (Sin Payload RT). Access Token huérfano.", currentUserId, currentUserSessionId, utcNow, asTracking, 
                                        cancellationToken);
                                    return Result.Ok();
                                }
                                else if (mismatchedUserSession is null && mismatchedUserSession1 is not null)
                                {
                                    _logger.LogWarning("Logout: Conflicto cruzado sin tokenPlain. Sesión de RT ({Session2}) inexistente. Revocando sesión de Access Token " + 
                                        "({Session1}). JTI: {Jti}, UserId: {UserId}.", userSessionRefreshToken.UserSessionId, currentUserSessionId, currentJti, currentUserId);
                                    await _userSessionService.RevokeUserSessionAndAccessTokenSessionAsync(mismatchedUserSession1, currentJti, currentAccessTokenExpiration,
                                        "Logout: Conflicto de sesión cruzada (Sin Payload RT). Refresh Token huérfano.", currentUserId, currentUserSessionId, utcNow, asTracking, 
                                        cancellationToken);
                                    return Result.Ok();
                                }
                                else
                                {
                                    _logger.LogWarning("Logout: Conflicto cruzado sin tokenPlain y ambas sesiones ({Session1}, {Session2}) son inexistentes. " + 
                                        "JTI: {Jti}, UserId: {UserId}.", currentUserSessionId, userSessionRefreshToken.UserSessionId, currentJti, currentUserId);
                                    await _userSessionService.BlacklistedAccessTokenSessionAsync(currentUserSessionId, currentJti, currentAccessTokenExpiration, currentUserId,
                                        "Logout: Conflicto de sesión cruzada (Sin Payload RT) y sesiones inexistentes.", utcNow, cancellationToken);
                                    return Result.Ok();
                                }
                            }
                        }
                    }
                    else
                    {
                        var mismatchedUserSession = await _userSessionRepository.GetByIdAsync(currentUserSessionId, asTracking, cancellationToken);
                        if (mismatchedUserSession is not null)
                        {
                            _logger.LogWarning("Logout: El identificador de Refresh Token provisto no existe en base de datos. " + 
                                "Revocando sesión actual {SessionId}. JTI: {Jti}, UserId: {UserId}.", currentUserSessionId, currentJti, currentUserId);
                            await _userSessionService.RevokeUserSessionAndAccessTokenSessionAsync(mismatchedUserSession, currentJti, currentAccessTokenExpiration,
                                "Logout: Identificador de Refresh Token no registrado en el sistema.", currentUserId, currentUserSessionId, utcNow, asTracking, cancellationToken);
                            return Result.Ok();
                        }
                        else
                        {
                            _logger.LogWarning("Logout: El identificador de Refresh Token provisto y la sesión actual {SessionId} no existen en base de datos. " + 
                                "JTI: {Jti}, UserId: {UserId}.", currentUserSessionId, currentJti, currentUserId);
                            await _userSessionService.BlacklistedAccessTokenSessionAsync(currentUserSessionId, currentJti, currentAccessTokenExpiration, currentUserId,
                                "Logout: Identificador de Refresh Token y Sesión no registrados en el sistema.", utcNow, cancellationToken);
                            return Result.Ok();
                        }
                    }
                }
                else
                {
                    var mismatchedUserSession = await _userSessionRepository.GetByIdAsync(currentUserSessionId, asTracking, cancellationToken);
                    if (mismatchedUserSession is not null)
                    {
                        _logger.LogWarning("Logout: No se pudo extraer el identificador del Refresh Token (Malformado). " + 
                            "Revocando sesión actual {SessionId}. JTI: {Jti}, UserId: {UserId}.", currentUserSessionId, currentJti, currentUserId);
                        await _userSessionService.RevokeUserSessionAndAccessTokenSessionAsync(mismatchedUserSession, currentJti, currentAccessTokenExpiration,
                            "Logout: Cadena de Refresh Token malformada.", currentUserId, currentUserSessionId, utcNow, asTracking, cancellationToken);
                        return Result.Ok();
                    }
                    else
                    {
                        _logger.LogWarning("Logout: Refresh Token malformado y sesión actual {SessionId} inexistente. JTI: {Jti}, UserId: {UserId}.", currentUserSessionId, 
                            currentJti, currentUserId);
                        await _userSessionService.BlacklistedAccessTokenSessionAsync(currentUserSessionId, currentJti, currentAccessTokenExpiration, currentUserId,
                            "Logout: Cadena de Refresh Token malformada y sesión inexistente.", utcNow, cancellationToken);
                        return Result.Ok();
                    }
                }
            }
            else
            {
                var mismatchedUserSession = await _userSessionRepository.GetByIdAsync(currentUserSessionId, asTracking, cancellationToken);
                if (mismatchedUserSession is not null)
                {
                    _logger.LogWarning("Logout: Se intentó cerrar sesión sin enviar un Refresh Token. " + 
                        "Revocando preventivamente la sesión {SessionId}. JTI: {Jti}, UserId: {UserId}.", currentUserSessionId, currentJti, currentUserId);
                    await _userSessionService.RevokeUserSessionAndAccessTokenSessionAsync(mismatchedUserSession, currentJti, currentAccessTokenExpiration,
                        "Logout: Cierre de sesión de tipo Session sin proveer Refresh Token.", currentUserId, currentUserSessionId, utcNow, asTracking, cancellationToken);
                    return Result.Ok();
                }
                else
                {
                    _logger.LogWarning("Logout: Cierre de sesión sin Refresh Token y la sesión {SessionId} del token ya no existe. JTI: {Jti}, UserId: {UserId}.", 
                        currentUserSessionId, currentJti, currentUserId);
                    await _userSessionService.BlacklistedAccessTokenSessionAsync(currentUserSessionId, currentJti, currentAccessTokenExpiration, currentUserId,
                        "Logout: Cierre sin Refresh Token para una sesión ya inexistente.", utcNow, cancellationToken);
                    return Result.Ok();
                }
            }
        }
    }
}