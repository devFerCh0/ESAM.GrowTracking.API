using ESAM.GrowTracking.Application.Abstractions.Services;
using ESAM.GrowTracking.Application.Enums;
using ESAM.GrowTracking.Application.Extensions;
using ESAM.GrowTracking.Application.Results;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ESAM.GrowTracking.Application.Features.Auth.Logout
{
    public sealed class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result>
    {
        private readonly ILogger<LogoutCommandHandler> _logger;
        private readonly IValidator<LogoutCommand> _validator;
        private readonly IAccessTokenClaimsValidatorService _accessTokenClaimsValidatorService;
        private readonly IDateTimeService _dateTimeService;
        private readonly IUserSessionService _userSessionService;
        private readonly IAuthSessionIntegrityValidatorService _authSessionIntegrityValidatorService;

        public LogoutCommandHandler(ILogger<LogoutCommandHandler> logger, IValidator<LogoutCommand> validator,
            IAccessTokenClaimsValidatorService accessTokenClaimsValidatorService, IDateTimeService dateTimeService, IUserSessionService userSessionService,
            IAuthSessionIntegrityValidatorService authSessionIntegrityValidatorService)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(validator);
            ArgumentNullException.ThrowIfNull(accessTokenClaimsValidatorService);
            ArgumentNullException.ThrowIfNull(dateTimeService);
            ArgumentNullException.ThrowIfNull(userSessionService);
            ArgumentNullException.ThrowIfNull(authSessionIntegrityValidatorService);
            _logger = logger;
            _validator = validator;
            _accessTokenClaimsValidatorService = accessTokenClaimsValidatorService;
            _dateTimeService = dateTimeService;
            _userSessionService = userSessionService;
            _authSessionIntegrityValidatorService = authSessionIntegrityValidatorService;
        }

        public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
        {
            var validation = await _validator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                _logger.LogWarning("LogoutCommand: Validación fallida. Errores: {Errors}", string.Join(" | ", validation.Errors.Select(e => e.ErrorMessage)));
                return Result.Fail(validation.ToCommandErrors());
            }
            var asTracking = false;
            var currentJti = _accessTokenClaimsValidatorService.CurrentJti;
            var currentUserId = _accessTokenClaimsValidatorService.CurrentUserId;
            var currentAccessTokenExpiration = _accessTokenClaimsValidatorService.CurrentAccessTokenExpiration;
            var utcNow = _dateTimeService.UtcNow;
            if (_accessTokenClaimsValidatorService.CurrentAccessTokenType == AccessTokenType.Temporary)
            {
                await _authSessionIntegrityValidatorService.ValidateTemporaryAccessTokenAsync("Logout", request.RefreshTokenRaw, currentJti, currentAccessTokenExpiration, 
                    currentUserId, utcNow, asTracking, cancellationToken);
                return Result.Ok();
            }
            var currentUserSessionId = _accessTokenClaimsValidatorService.CurrentUserSessionId;
            var currentUserDeviceId = _accessTokenClaimsValidatorService.CurrentUserDeviceId;
            var integrityResult = await _authSessionIntegrityValidatorService.ValidateSessionAccessTokenAsync("Logout", request.RefreshTokenRaw, currentJti, 
                currentAccessTokenExpiration, currentUserId, currentUserSessionId, currentUserDeviceId, utcNow, asTracking, cancellationToken);
            if (integrityResult.IsValid)
            {
                _logger.LogInformation("Logout: Sesión {SessionId} y AT (JTI: {Jti}) revocados exitosamente para usuario {UserId}.", currentUserSessionId, currentJti, 
                    currentUserId);
                await _userSessionService.RevokeUserSessionAndAccessTokenSessionAsync(integrityResult.UserSession!, currentJti, currentAccessTokenExpiration,
                    "Logout: Cierre de sesión exitoso.", currentUserId, currentUserSessionId, utcNow, asTracking, cancellationToken);
            }
            return Result.Ok();
        }
    }

    //public sealed class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result>
    //{
    //    private readonly ILogger<LogoutCommandHandler> _logger;
    //    private readonly IValidator<LogoutCommand> _validator;
    //    private readonly IAccessTokenClaimsValidatorService _accessTokenClaimsValidatorService;
    //    private readonly IDateTimeService _dateTimeService;
    //    private readonly IUserSessionRefreshTokenRepository _userSessionRefreshTokenRepository;
    //    private readonly IUserSessionRepository _userSessionRepository;
    //    private readonly IUserSessionService _userSessionService;
    //    private readonly IHashService _hashService;

    //    public LogoutCommandHandler(ILogger<LogoutCommandHandler> logger, IValidator<LogoutCommand> validator, IAccessTokenClaimsValidatorService accessTokenClaimsValidatorService,
    //        IDateTimeService dateTimeService, IUserSessionRefreshTokenRepository userSessionRefreshTokenRepository, IUserSessionRepository userSessionRepository, 
    //        IUserSessionService userSessionService, IHashService hashService)
    //    {
    //        ArgumentNullException.ThrowIfNull(logger);
    //        ArgumentNullException.ThrowIfNull(validator);
    //        ArgumentNullException.ThrowIfNull(accessTokenClaimsValidatorService);
    //        ArgumentNullException.ThrowIfNull(dateTimeService);
    //        ArgumentNullException.ThrowIfNull(userSessionRefreshTokenRepository);
    //        ArgumentNullException.ThrowIfNull(userSessionRepository);
    //        ArgumentNullException.ThrowIfNull(userSessionService);
    //        ArgumentNullException.ThrowIfNull(hashService);
    //        _logger = logger;
    //        _validator = validator;
    //        _accessTokenClaimsValidatorService = accessTokenClaimsValidatorService;
    //        _dateTimeService = dateTimeService;
    //        _userSessionRefreshTokenRepository = userSessionRefreshTokenRepository;
    //        _userSessionRepository = userSessionRepository;
    //        _userSessionService = userSessionService;
    //        _hashService = hashService;
    //    }

    //    public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
    //    {
    //        var validation = await _validator.ValidateAsync(request, cancellationToken);
    //        if (!validation.IsValid)
    //        {
    //            _logger.LogWarning("LogoutCommand: Validación fallida. Errores: {Errors}", string.Join(" | ", validation.Errors.Select(e => e.ErrorMessage)));
    //            return Result.Fail(validation.ToCommandErrors());
    //        }
    //        var currentAccessTokenType = _accessTokenClaimsValidatorService.CurrentAccessTokenType;
    //        var asTracking = false;
    //        var currentUserId = _accessTokenClaimsValidatorService.CurrentUserId;
    //        var currentJti = _accessTokenClaimsValidatorService.CurrentJti;
    //        var currentAccessTokenExpiration = _accessTokenClaimsValidatorService.CurrentAccessTokenExpiration;
    //        var utcNow = _dateTimeService.UtcNow;
    //        if (currentAccessTokenType == AccessTokenType.Temporary)
    //            return await LogoutAccessTokenTemporary(request.RefreshTokenRaw, currentJti, currentAccessTokenExpiration, currentUserId, utcNow, asTracking, cancellationToken);
    //        else
    //        {
    //            var currentUserSessionId = _accessTokenClaimsValidatorService.CurrentUserSessionId;
    //            var currentUserDeviceId = _accessTokenClaimsValidatorService.CurrentUserDeviceId;
    //            return await LogoutAccessTokenSession(request.RefreshTokenRaw, currentJti, currentAccessTokenExpiration, currentUserId, currentUserSessionId, currentUserDeviceId, 
    //                utcNow, asTracking, cancellationToken);
    //        }
    //    }

    //    private async Task<Result> LogoutAccessTokenTemporary(string? refreshTokenRaw, string currentJti, DateTime currentAccessTokenExpiration, int currentUserId, DateTime utcNow, 
    //        bool asTracking = false, CancellationToken cancellationToken = default)
    //    {
    //        if (refreshTokenRaw is not null)
    //        {
    //            RefreshTokenParser.TryParse(refreshTokenRaw, out var identifier, out _);
    //            if (identifier is not null)
    //            {
    //                var userSessionRefreshToken = await _userSessionRefreshTokenRepository.GetByIdentifierAsync(identifier, asTracking, cancellationToken);
    //                if (userSessionRefreshToken is not null)
    //                {
    //                    var mismatchedUserSession = await _userSessionRepository.GetByIdAsync(userSessionRefreshToken.UserSessionId, asTracking, cancellationToken);
    //                    if (mismatchedUserSession is not null)
    //                    {
    //                        _logger.LogWarning("Logout: Token Temporal (JTI: {Jti}) con RT válido de sesión {SessionId} (UserId: {UserId}).", currentJti, mismatchedUserSession.Id, 
    //                            currentUserId);
    //                        await _userSessionService.RevokeUserSessionAndAccessTokenTemporaryAsync(mismatchedUserSession, currentJti, currentAccessTokenExpiration,
    //                            "Logout: Uso anómalo de RT con Token Temporal.", currentUserId, utcNow, asTracking, cancellationToken);
    //                        return Result.Ok();
    //                    }
    //                    else
    //                    {
    //                        _logger.LogWarning("Logout: Token Temporal (JTI: {Jti}) con RT de sesión inexistente (UserId: {UserId}).", currentJti, currentUserId);
    //                        await _userSessionService.RevokeAccessTokenTemporaryAsync(currentUserId, currentJti, currentAccessTokenExpiration,
    //                            "Logout: Uso de RT huérfano con Token Temporal.", utcNow, asTracking, cancellationToken);
    //                        return Result.Ok();
    //                    }
    //                }
    //                else
    //                {
    //                    _logger.LogWarning("Logout: Token Temporal (JTI: {Jti}) con RT inexistente (UserId: {UserId}).", currentJti, currentUserId);
    //                    await _userSessionService.RevokeAccessTokenTemporaryAsync(currentUserId, currentJti, currentAccessTokenExpiration,
    //                        "Logout: Uso de RT no registrado con Token Temporal.", utcNow, asTracking, cancellationToken);
    //                    return Result.Ok();
    //                }
    //            }
    //            else
    //            {
    //                _logger.LogWarning("Logout: Token Temporal (JTI: {Jti}) con RT malformado (UserId: {UserId}).", currentJti, currentUserId);
    //                await _userSessionService.RevokeAccessTokenTemporaryAsync(currentUserId, currentJti, currentAccessTokenExpiration,
    //                    "Logout: Uso de RT malformado con Token Temporal.", utcNow, asTracking, cancellationToken);
    //                return Result.Ok();
    //            }
    //        }
    //        else
    //        {
    //            _logger.LogInformation("Logout: Token Temporal (JTI: {Jti}) revocado exitosamente para usuario {UserId}.", currentJti, currentUserId);
    //            await _userSessionService.RevokeAccessTokenTemporaryAsync(currentUserId, currentJti, currentAccessTokenExpiration,
    //                "Logout: Cierre de sesión temporal exitoso.", utcNow, asTracking, cancellationToken);
    //            return Result.Ok();
    //        }
    //    }

    //    private async Task<Result> LogoutAccessTokenSession(string? refreshTokenRaw, string currentJti, DateTime currentAccessTokenExpiration, int currentUserId, 
    //        int currentUserSessionId, int currentUserDeviceId, DateTime utcNow, bool asTracking = false, CancellationToken cancellationToken = default)
    //    {
    //        if (refreshTokenRaw is not null)
    //        {
    //            RefreshTokenParser.TryParse(refreshTokenRaw, out var identifier, out var tokenPlain);
    //            if (identifier is not null)
    //            {
    //                var userSessionRefreshToken = await _userSessionRefreshTokenRepository.GetByIdentifierAsync(identifier, asTracking, cancellationToken);
    //                if (userSessionRefreshToken is not null)
    //                {
    //                    if (tokenPlain is not null)
    //                    {
    //                        var computedHash = _hashService.ComputeHash(tokenPlain, userSessionRefreshToken.Salt);
    //                        if (CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(computedHash), Encoding.UTF8.GetBytes(userSessionRefreshToken.TokenHash)))
    //                        {
    //                            var userSession = await _userSessionRepository.GetByIdAndUserIdAndUserDeviceIdAsync(currentUserSessionId, currentUserId, currentUserDeviceId,
    //                                asTracking, cancellationToken);
    //                            if (userSession is not null)
    //                            {
    //                                if (userSession.AbsoluteExpiresAt <= utcNow)
    //                                {
    //                                    _logger.LogWarning("Logout: Sesión {SessionId} superó expiración absoluta. JTI: {Jti}, UserId: {UserId}.", currentUserSessionId, currentJti, 
    //                                        currentUserId);
    //                                    await _userSessionService.RevokeUserSessionAndAccessTokenSessionAsync(userSession, currentJti, currentAccessTokenExpiration,
    //                                        "Logout: Expiración absoluta alcanzada.", currentUserId, currentUserSessionId, utcNow, asTracking, cancellationToken);
    //                                    return Result.Ok();
    //                                }
    //                                else if (userSession.ExpiresAt <= utcNow || userSessionRefreshToken.ExpiresAt <= utcNow)
    //                                {
    //                                    _logger.LogWarning("Logout: Sesión {SessionId} o RT expirados por inactividad. JTI: {Jti}, UserId: {UserId}.", currentUserSessionId,
    //                                        currentJti, currentUserId);
    //                                    await _userSessionService.RevokeUserSessionAndAccessTokenSessionAsync(userSession, currentJti, currentAccessTokenExpiration,
    //                                        "Logout: Tiempo de inactividad superado.", currentUserId, currentUserSessionId, utcNow, asTracking, cancellationToken);
    //                                    return Result.Ok();
    //                                }
    //                                else if (userSession.IsRevoked || userSessionRefreshToken.IsRevoked)
    //                                {
    //                                    _logger.LogWarning("Logout: Intento sobre sesión {SessionId} o RT ya revocados. JTI: {Jti}, UserId: {UserId}.", currentUserSessionId,
    //                                        currentJti, currentUserId);
    //                                    await _userSessionService.RevokeUserSessionAndAccessTokenSessionAsync(userSession, currentJti, currentAccessTokenExpiration,
    //                                        "Logout: Cierre sobre credenciales previamente revocadas.", currentUserId, currentUserSessionId, utcNow, asTracking,
    //                                        cancellationToken);
    //                                    return Result.Ok();
    //                                }
    //                                else if (userSessionRefreshToken.ReplacedByUserSessionRefreshTokenId.HasValue)
    //                                {
    //                                    _logger.LogWarning("Logout: Ataque de repetición. RT rotado en sesión {SessionId}. JTI: {Jti}, UserId: {UserId}.", currentUserSessionId,
    //                                        currentJti, currentUserId);
    //                                    await _userSessionService.RevokeUserSessionAndAccessTokenSessionAsync(userSession, currentJti, currentAccessTokenExpiration,
    //                                        "Logout: Posible ataque de repetición. RT reemplazado.", currentUserId, currentUserSessionId, utcNow, asTracking,
    //                                        cancellationToken);
    //                                    return Result.Ok();
    //                                }
    //                                else
    //                                {
    //                                    _logger.LogInformation("Logout: Sesión {SessionId} y AT (JTI: {Jti}) revocados exitosamente para usuario {UserId}.", currentUserSessionId, 
    //                                        currentJti, currentUserId);
    //                                    await _userSessionService.RevokeUserSessionAndAccessTokenSessionAsync(userSession, currentJti, currentAccessTokenExpiration,
    //                                        "Logout: Cierre de sesión exitoso.", currentUserId, currentUserSessionId, utcNow, asTracking, cancellationToken);
    //                                    return Result.Ok();
    //                                }
    //                            }
    //                            else
    //                            {
    //                                _logger.LogWarning("Logout: Sesión {SessionId} o dispositivo {DeviceId} no coinciden. JTI: {Jti}, UserId: {UserId}.", currentUserSessionId,
    //                                    currentUserDeviceId, currentJti, currentUserId);
    //                                await _userSessionService.RevokeAccessTokenSessionAsync(currentUserSessionId, currentJti, currentAccessTokenExpiration, currentUserId,
    //                                    "Logout: Datos de dispositivo o sesión no coinciden.", utcNow, asTracking, cancellationToken);
    //                                return Result.Ok();
    //                            }
    //                        }
    //                        else
    //                        {
    //                            if (userSessionRefreshToken.UserSessionId == currentUserSessionId)
    //                            {
    //                                var mismatchedUserSession = await _userSessionRepository.GetByIdAsync(currentUserSessionId, asTracking, cancellationToken);
    //                                if (mismatchedUserSession is not null)
    //                                {
    //                                    _logger.LogWarning("Logout: Fallo de hash en RT para sesión {SessionId}. JTI: {Jti}, UserId: {UserId}.", currentUserSessionId, currentJti, 
    //                                        currentUserId);
    //                                    await _userSessionService.RevokeUserSessionAndAccessTokenSessionAsync(mismatchedUserSession, currentJti, currentAccessTokenExpiration,
    //                                        "Logout: Discrepancia criptográfica (Fallo de Hash).", currentUserId, currentUserSessionId, utcNow, asTracking, cancellationToken);
    //                                    return Result.Ok();
    //                                }
    //                                else
    //                                {
    //                                    _logger.LogWarning("Logout: Fallo de hash y sesión {SessionId} inexistente. JTI: {Jti}, UserId: {UserId}.", currentUserSessionId, 
    //                                        currentJti, currentUserId);
    //                                    await _userSessionService.RevokeAccessTokenSessionAsync(currentUserSessionId, currentJti, currentAccessTokenExpiration, currentUserId, 
    //                                        "Logout: Discrepancia criptográfica y sesión no encontrada.", utcNow, asTracking, cancellationToken);
    //                                    return Result.Ok();
    //                                }
    //                            }
    //                            else
    //                            {
    //                                var mismatchedUserSession = await _userSessionRepository.GetByIdAsync(userSessionRefreshToken.UserSessionId, asTracking, cancellationToken);
    //                                var mismatchedUserSession1 = await _userSessionRepository.GetByIdAsync(currentUserSessionId, asTracking, cancellationToken);
    //                                if (mismatchedUserSession is not null && mismatchedUserSession1 is not null)
    //                                {
    //                                    _logger.LogWarning("Logout: Conflicto cruzado con fallo de hash. AT: {Session1}, RT: {Session2}. JTI: {Jti}, UserId: {UserId}.",
    //                                        currentUserSessionId, userSessionRefreshToken.UserSessionId, currentJti, currentUserId);
    //                                    await _userSessionService.RevokeUserSessionAndAccessTokenSessionAsync(mismatchedUserSession, mismatchedUserSession1, currentJti,
    //                                        currentAccessTokenExpiration, "Logout: Conflicto de sesión cruzada y fallo criptográfico.", currentUserId, currentUserSessionId, utcNow, 
    //                                        asTracking, cancellationToken);
    //                                    return Result.Ok();
    //                                }
    //                                else if (mismatchedUserSession is not null && mismatchedUserSession1 is null)
    //                                {
    //                                    _logger.LogWarning("Logout: Sesión cruzada. AT huérfano y hash inválido. AT: {Session1}, RT: {Session2}. JTI: {Jti}, UserId: {UserId}.",
    //                                        currentUserSessionId, userSessionRefreshToken.UserSessionId, currentJti, currentUserId);
    //                                    await _userSessionService.RevokeUserSessionAndAccessTokenSessionAsync(mismatchedUserSession, currentJti, currentAccessTokenExpiration,
    //                                        "Logout: Conflicto cruzado y AT huérfano.", currentUserId, currentUserSessionId, utcNow, asTracking, cancellationToken);
    //                                    return Result.Ok();
    //                                }
    //                                else if (mismatchedUserSession is null && mismatchedUserSession1 is not null)
    //                                {
    //                                    _logger.LogWarning("Logout: Sesión cruzada. RT huérfano y hash inválido. AT: {Session1}, RT: {Session2}. JTI: {Jti}, UserId: {UserId}.",
    //                                        currentUserSessionId, userSessionRefreshToken.UserSessionId, currentJti, currentUserId);
    //                                    await _userSessionService.RevokeUserSessionAndAccessTokenSessionAsync(mismatchedUserSession1, currentJti, currentAccessTokenExpiration,
    //                                        "Logout: Conflicto cruzado y RT huérfano.", currentUserId, currentUserSessionId, utcNow, asTracking, cancellationToken);
    //                                    return Result.Ok();
    //                                }
    //                                else
    //                                {
    //                                    _logger.LogWarning("Logout: Conflicto cruzado total. Sesiones inexistentes y hash inválido. AT: {Session1}, RT: {Session2}. " +
    //                                        "JTI: {Jti}, UserId: {UserId}.", currentUserSessionId, userSessionRefreshToken.UserSessionId, currentJti, currentUserId);
    //                                    await _userSessionService.RevokeAccessTokenSessionAsync(currentUserSessionId, currentJti, currentAccessTokenExpiration,
    //                                        currentUserId, "Logout: Conflicto cruzado y sesiones inexistentes.", utcNow, asTracking, cancellationToken);
    //                                    return Result.Ok();
    //                                }
    //                            }
    //                        }
    //                    }
    //                    else
    //                    {
    //                        if (userSessionRefreshToken.UserSessionId == currentUserSessionId)
    //                        {
    //                            var mismatchedUserSession = await _userSessionRepository.GetByIdAsync(currentUserSessionId, asTracking, cancellationToken);
    //                            if (mismatchedUserSession is not null)
    //                            {
    //                                _logger.LogWarning("Logout: Payload de RT ausente para sesión {SessionId}. JTI: {Jti}, UserId: {UserId}.", currentUserSessionId, currentJti,
    //                                    currentUserId);
    //                                await _userSessionService.RevokeUserSessionAndAccessTokenSessionAsync(mismatchedUserSession, currentJti, currentAccessTokenExpiration,
    //                                    "Logout: Payload de Refresh Token ausente.", currentUserId, currentUserSessionId, utcNow, asTracking, cancellationToken);
    //                                return Result.Ok();
    //                            }
    //                            else
    //                            {
    //                                _logger.LogWarning("Logout: Payload de RT ausente y sesión {SessionId} inexistente. JTI: {Jti}, UserId: {UserId}.", currentUserSessionId,
    //                                    currentJti, currentUserId);
    //                                await _userSessionService.RevokeAccessTokenSessionAsync(currentUserSessionId, currentJti, currentAccessTokenExpiration, currentUserId,
    //                                    "Logout: Payload ausente y sesión no encontrada.", utcNow, asTracking, cancellationToken);
    //                                return Result.Ok();
    //                            }
    //                        }
    //                        else
    //                        {
    //                            var mismatchedUserSession = await _userSessionRepository.GetByIdAsync(userSessionRefreshToken.UserSessionId, asTracking, cancellationToken);
    //                            var mismatchedUserSession1 = await _userSessionRepository.GetByIdAsync(currentUserSessionId, asTracking, cancellationToken);
    //                            if (mismatchedUserSession is not null && mismatchedUserSession1 is not null)
    //                            {
    //                                _logger.LogWarning("Logout: Conflicto cruzado sin payload en RT. AT: {Session1}, RT: {Session2}. JTI: {Jti}, UserId: {UserId}.", 
    //                                    currentUserSessionId, userSessionRefreshToken.UserSessionId, currentJti, currentUserId);
    //                                await _userSessionService.RevokeUserSessionAndAccessTokenSessionAsync(mismatchedUserSession, mismatchedUserSession1, currentJti, 
    //                                    currentAccessTokenExpiration, "Logout: Conflicto cruzado sin Payload RT.", currentUserId, currentUserSessionId, utcNow, asTracking,
    //                                    cancellationToken);
    //                                return Result.Ok();
    //                            }
    //                            else if (mismatchedUserSession is not null && mismatchedUserSession1 is null)
    //                            {
    //                                _logger.LogWarning("Logout: Conflicto cruzado sin payload. AT huérfano. AT: {Session1}, RT: {Session2}. JTI: {Jti}, UserId: {UserId}.",
    //                                    currentUserSessionId, userSessionRefreshToken.UserSessionId, currentJti, currentUserId);
    //                                await _userSessionService.RevokeUserSessionAndAccessTokenSessionAsync(mismatchedUserSession, currentJti, currentAccessTokenExpiration,
    //                                    "Logout: Conflicto cruzado sin Payload. AT huérfano.", currentUserId, currentUserSessionId, utcNow, asTracking, cancellationToken);
    //                                return Result.Ok();
    //                            }
    //                            else if (mismatchedUserSession is null && mismatchedUserSession1 is not null)
    //                            {
    //                                _logger.LogWarning("Logout: Conflicto cruzado sin payload. RT huérfano. AT: {Session1}, RT: {Session2}. JTI: {Jti}, UserId: {UserId}.",
    //                                    currentUserSessionId, userSessionRefreshToken.UserSessionId, currentJti, currentUserId);
    //                                await _userSessionService.RevokeUserSessionAndAccessTokenSessionAsync(mismatchedUserSession1, currentJti, currentAccessTokenExpiration,
    //                                    "Logout: Conflicto cruzado sin Payload. RT huérfano.", currentUserId, currentUserSessionId, utcNow, asTracking, cancellationToken);
    //                                return Result.Ok();
    //                            }
    //                            else
    //                            {
    //                                _logger.LogWarning("Logout: Conflicto cruzado sin payload, sesiones inexistentes. AT: {Session1}, RT: {Session2}. JTI: {Jti}, " +
    //                                    "UserId: {UserId}.", currentUserSessionId, userSessionRefreshToken.UserSessionId, currentJti, currentUserId);
    //                                await _userSessionService.RevokeAccessTokenSessionAsync(currentUserSessionId, currentJti, currentAccessTokenExpiration, currentUserId,
    //                                    "Logout: Conflicto cruzado sin Payload y sin sesiones.", utcNow, asTracking, cancellationToken);
    //                                return Result.Ok();
    //                            }
    //                        }
    //                    }
    //                }
    //                else
    //                {
    //                    var mismatchedUserSession = await _userSessionRepository.GetByIdAsync(currentUserSessionId, asTracking, cancellationToken);
    //                    if (mismatchedUserSession is not null)
    //                    {
    //                        _logger.LogWarning("Logout: RT no registrado en BD para sesión {SessionId}. JTI: {Jti}, UserId: {UserId}.", currentUserSessionId, currentJti, 
    //                            currentUserId);
    //                        await _userSessionService.RevokeUserSessionAndAccessTokenSessionAsync(mismatchedUserSession, currentJti, currentAccessTokenExpiration,
    //                            "Logout: Identificador de Refresh Token no registrado.", currentUserId, currentUserSessionId, utcNow, asTracking, cancellationToken);
    //                        return Result.Ok();
    //                    }
    //                    else
    //                    {
    //                        _logger.LogWarning("Logout: RT no registrado y sesión {SessionId} inexistente. JTI: {Jti}, UserId: {UserId}.", currentUserSessionId, currentJti,
    //                            currentUserId);
    //                        await _userSessionService.RevokeAccessTokenSessionAsync(currentUserSessionId, currentJti, currentAccessTokenExpiration, currentUserId,
    //                            "Logout: RT y sesión no registrados.", utcNow, asTracking, cancellationToken);
    //                        return Result.Ok();
    //                    }
    //                }
    //            }
    //            else
    //            {
    //                var mismatchedUserSession = await _userSessionRepository.GetByIdAsync(currentUserSessionId, asTracking, cancellationToken);
    //                if (mismatchedUserSession is not null)
    //                {
    //                    _logger.LogWarning("Logout: Identificador de RT malformado para sesión {SessionId}. JTI: {Jti}, UserId: {UserId}.", currentUserSessionId, currentJti,
    //                        currentUserId);
    //                    await _userSessionService.RevokeUserSessionAndAccessTokenSessionAsync(mismatchedUserSession, currentJti, currentAccessTokenExpiration,
    //                        "Logout: Refresh Token malformado.", currentUserId, currentUserSessionId, utcNow, asTracking, cancellationToken);
    //                    return Result.Ok();
    //                }
    //                else
    //                {
    //                    _logger.LogWarning("Logout: RT malformado y sesión {SessionId} inexistente. JTI: {Jti}, UserId: {UserId}.", currentUserSessionId, currentJti, 
    //                        currentUserId);
    //                    await _userSessionService.RevokeAccessTokenSessionAsync(currentUserSessionId, currentJti, currentAccessTokenExpiration, currentUserId,
    //                        "Logout: RT malformado y sesión inexistente.", utcNow, asTracking, cancellationToken);
    //                    return Result.Ok();
    //                }
    //            }
    //        }
    //        else
    //        {
    //            var mismatchedUserSession = await _userSessionRepository.GetByIdAsync(currentUserSessionId, asTracking, cancellationToken);
    //            if (mismatchedUserSession is not null)
    //            {
    //                _logger.LogWarning("Logout: Cierre sin enviar RT para sesión {SessionId}. JTI: {Jti}, UserId: {UserId}.", currentUserSessionId, currentJti, currentUserId);
    //                await _userSessionService.RevokeUserSessionAndAccessTokenSessionAsync(mismatchedUserSession, currentJti, currentAccessTokenExpiration,
    //                    "Logout: Petición sin Refresh Token.", currentUserId, currentUserSessionId, utcNow, asTracking, cancellationToken);
    //                return Result.Ok();
    //            }
    //            else
    //            {
    //                _logger.LogWarning("Logout: Cierre sin RT y sesión {SessionId} inexistente. JTI: {Jti}, UserId: {UserId}.", currentUserSessionId, currentJti, currentUserId);
    //                await _userSessionService.RevokeAccessTokenSessionAsync(currentUserSessionId, currentJti, currentAccessTokenExpiration, currentUserId,
    //                    "Logout: Petición sin RT para sesión inexistente.", utcNow, asTracking, cancellationToken);
    //                return Result.Ok();
    //            }
    //        }
    //    }
    //}
}