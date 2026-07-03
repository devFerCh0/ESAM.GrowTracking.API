using ESAM.GrowTracking.Application.Abstractions.Services;
using ESAM.GrowTracking.Application.Enums;
using ESAM.GrowTracking.Application.Extensions;
using ESAM.GrowTracking.Application.Helpers;
using ESAM.GrowTracking.Application.Results;
using ESAM.GrowTracking.Application.ValueObjects;
using ESAM.GrowTracking.Domain.Abstractions.DataAccess.Repositories;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace ESAM.GrowTracking.Application.Features.Auth.Refresh
{
    public class RefreshCommandHandler : IRequestHandler<RefreshCommand, Result<RefreshResponse>>
    {
        private readonly ILogger<RefreshCommandHandler> _logger;
        private readonly IValidator<RefreshCommand> _validator;
        private readonly IAccessTokenClaimsValidatorService _accessTokenClaimsValidatorService;
        private readonly IDateTimeService _dateTimeService;
        private readonly IUserSessionRefreshTokenRepository _userSessionRefreshTokenRepository;
        private readonly IUserSessionService _userSessionService;
        private readonly IUserSessionRepository _userSessionRepository;
        private readonly IHashService _hashService;

        public RefreshCommandHandler(ILogger<RefreshCommandHandler> logger, IValidator<RefreshCommand> validator, 
            IAccessTokenClaimsValidatorService accessTokenClaimsValidatorService, IDateTimeService dateTimeService, 
            IUserSessionRefreshTokenRepository userSessionRefreshTokenRepository, IUserSessionService userSessionService, IUserSessionRepository userSessionRepository,
            IHashService hashService)

        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(validator);
            ArgumentNullException.ThrowIfNull(accessTokenClaimsValidatorService);
            ArgumentNullException.ThrowIfNull(dateTimeService);
            ArgumentNullException.ThrowIfNull(userSessionRefreshTokenRepository);
            ArgumentNullException.ThrowIfNull(userSessionService);
            ArgumentNullException.ThrowIfNull(userSessionRepository);
            ArgumentNullException.ThrowIfNull(hashService);
            _logger = logger;
            _validator = validator;
            _accessTokenClaimsValidatorService = accessTokenClaimsValidatorService;
            _dateTimeService = dateTimeService;
            _userSessionRefreshTokenRepository = userSessionRefreshTokenRepository;
            _userSessionService = userSessionService;
            _userSessionRepository = userSessionRepository;
            _hashService = hashService;

            //ArgumentNullException.ThrowIfNull(currentUserService);
            //ArgumentNullException.ThrowIfNull(blacklistedTokenService);
            //ArgumentNullException.ThrowIfNull(unitOfWork);
            //ArgumentNullException.ThrowIfNull(tokenService);
            //ArgumentNullException.ThrowIfNull(tokenLifetimeSettingsOptions);
            //ArgumentNullException.ThrowIfNull(tokenSessionValidationService);
            //_currentUserService = currentUserService;
            //_blacklistedTokenService = blacklistedTokenService;
            //_unitOfWork = unitOfWork;
            //_tokenService = tokenService;
            //_tokenLifetimeSettings = tokenLifetimeSettingsOptions.Value ?? throw new ArgumentNullException(nameof(tokenLifetimeSettingsOptions));
            //_tokenSessionValidationService = tokenSessionValidationService;
        }

        public async Task<Result<RefreshResponse>> Handle(RefreshCommand request, CancellationToken cancellationToken)
        {
            var validation = await _validator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                _logger.LogWarning("LogoutCommand: Validación fallida. Errores: {Errors}", string.Join(" | ", validation.Errors.Select(e => e.ErrorMessage)));
                return Result<RefreshResponse>.Fail(validation.ToCommandErrors());
            }
            await _accessTokenClaimsValidatorService.UseExplicitAccessTokenAsync(request.AccessToken);
            var currentAccessTokenType = _accessTokenClaimsValidatorService.CurrentAccessTokenType;
            var currentJti = _accessTokenClaimsValidatorService.CurrentJti;
            var currentAccessTokenExpiration = _accessTokenClaimsValidatorService.CurrentAccessTokenExpiration;
            var currentUserId = _accessTokenClaimsValidatorService.CurrentUserId;
            var utcNow = _dateTimeService.UtcNow;
            var asTracking = false;
            if (currentAccessTokenType == AccessTokenType.Temporary)
                return await DenyRefreshAccessTokenTemporary(request.RefreshTokenRaw, currentJti, currentAccessTokenExpiration, currentUserId, utcNow, asTracking, 
                    cancellationToken);
            else
            {
                var currentUserSessionId = _accessTokenClaimsValidatorService.CurrentUserSessionId;
                var currentUserDeviceId = _accessTokenClaimsValidatorService.CurrentUserDeviceId;
                return await RefreshAccessTokenSession(request.RefreshTokenRaw, currentJti, currentAccessTokenExpiration, currentUserId, currentUserSessionId, currentUserDeviceId,
                    utcNow, asTracking, cancellationToken);
            }
        }

        private async Task<Result<RefreshResponse>> DenyRefreshAccessTokenTemporary(string refreshTokenRaw, string currentJti, DateTime currentAccessTokenExpiration, 
            int currentUserId, DateTime utcNow, bool asTracking = false, CancellationToken cancellationToken = default)
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
                        _logger.LogWarning("Refresh: Intento con Token Temporal (JTI: {Jti}) y RT válido de sesión {SessionId}.", currentJti, mismatchedUserSession.Id);
                        await _userSessionService.RevokeUserSessionAndAccessTokenTemporaryAsync(mismatchedUserSession, currentJti, currentAccessTokenExpiration, 
                            "Refresh: Uso de Refresh Token con Access Token Temporal.", currentUserId, utcNow, asTracking, cancellationToken);
                        return Result<RefreshResponse>.Fail(Error.Forbidden("Renovación no permitida para tokens temporales. Sesión comprometida."));
                    }
                    else
                    {
                        _logger.LogWarning("Refresh: Intento con Token Temporal (JTI: {Jti}) y RT de sesión inexistente.", currentJti);
                        await _userSessionService.BlacklistedAccessTokenTemporaryAsync(currentUserId, currentJti, currentAccessTokenExpiration, 
                            "Refresh: Uso de Refresh Token huérfano con Access Token Temporal.", utcNow, cancellationToken);
                        return Result<RefreshResponse>.Fail(Error.Forbidden("Renovación denegada. Token temporal inválido."));
                    }
                }
                else
                {
                    _logger.LogWarning("Refresh: Intento con Token Temporal (JTI: {Jti}) y RT inexistente.", currentJti);
                    await _userSessionService.BlacklistedAccessTokenTemporaryAsync(currentUserId, currentJti, currentAccessTokenExpiration, 
                        "Refresh: Uso de Refresh Token no registrado con Access Token Temporal.", utcNow, cancellationToken);
                    return Result<RefreshResponse>.Fail(Error.Forbidden("Renovación denegada. Credenciales no registradas."));
                }
            }
            else
            {
                _logger.LogWarning("Refresh: Intento con Token Temporal (JTI: {Jti}) y RT malformado.", currentJti);
                await _userSessionService.BlacklistedAccessTokenTemporaryAsync(currentUserId, currentJti, currentAccessTokenExpiration, 
                    "Refresh: Uso de Refresh Token malformado con Access Token Temporal.", utcNow, cancellationToken);
                return Result<RefreshResponse>.Fail(Error.Validation("Formato de token inválido."));
            }
        }

        private async Task<Result<RefreshResponse>> RefreshAccessTokenSession(string refreshTokenRaw, string currentJti, DateTime currentAccessTokenExpiration, int currentUserId,
            int currentUserSessionId, int currentUserDeviceId, DateTime utcNow, bool asTracking = false, CancellationToken cancellationToken = default)
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
                                    _logger.LogWarning("Refresh: Sesión {SessionId} alcanzó expiración absoluta. JTI: {Jti}.", currentUserSessionId, currentJti);
                                    await _userSessionService.RevokeUserSessionAndAccessTokenSessionAsync(userSession, currentJti, currentAccessTokenExpiration,
                                        "Refresh: Expiración absoluta alcanzada.", currentUserId, currentUserSessionId, utcNow, asTracking, cancellationToken);
                                    return Result<RefreshResponse>.Fail(Error.Unauthorized("La sesión ha expirado permanentemente."));
                                }
                                else if (userSession.ExpiresAt <= utcNow || userSessionRefreshToken.ExpiresAt <= utcNow)
                                {
                                    _logger.LogWarning("Refresh: Sesión {SessionId} o RT expirados. JTI: {Jti}.", currentUserSessionId, currentJti);
                                    await _userSessionService.RevokeUserSessionAndAccessTokenSessionAsync(userSession, currentJti, currentAccessTokenExpiration,
                                        "Refresh: Tiempo de inactividad superado.", currentUserId, currentUserSessionId, utcNow, asTracking, cancellationToken);
                                    return Result<RefreshResponse>.Fail(Error.Unauthorized("La sesión por inactividad ha expirado."));
                                }
                                else if (userSession.IsRevoked || userSessionRefreshToken.IsRevoked)
                                {
                                    _logger.LogWarning("Refresh: Intento sobre sesión {SessionId} o RT ya revocados. JTI: {Jti}.", currentUserSessionId, currentJti);
                                    await _userSessionService.RevokeUserSessionAndAccessTokenSessionAsync(userSession, currentJti, currentAccessTokenExpiration,
                                        "Refresh: Uso de credenciales previamente revocadas.", currentUserId, currentUserSessionId, utcNow, asTracking, cancellationToken);
                                    return Result<RefreshResponse>.Fail(Error.Unauthorized("La sesión ya fue terminada previamente."));
                                }
                                else if (userSessionRefreshToken.ReplacedByUserSessionRefreshTokenId.HasValue)
                                {
                                    _logger.LogWarning("Refresh: Posible ataque de repetición. RT rotado en sesión {SessionId}. JTI: {Jti}.", currentUserSessionId, currentJti);
                                    await _userSessionService.RevokeUserSessionAndAccessTokenSessionAsync(userSession, currentJti, currentAccessTokenExpiration,
                                        "Refresh: Ataque de repetición. Uso de RT reemplazado.", currentUserId, currentUserSessionId, utcNow, asTracking, cancellationToken);
                                    return Result<RefreshResponse>.Fail(Error.Forbidden("Detectada anomalía de seguridad. Sesión invalidada."));
                                }
                                else
                                {
                                    _logger.LogInformation("Logout: Sesión {SessionId} y Access Token (JTI: {Jti}) revocados correctamente para el usuario " +
                                        "{UserId}.", currentUserSessionId, currentJti, currentUserId);
                                    await _userSessionService.RevokeUserSessionAndAccessTokenSessionAsync(userSession, currentJti, currentAccessTokenExpiration,
                                        "Logout: Cierre de sesión exitoso.", currentUserId, currentUserSessionId, utcNow, asTracking, cancellationToken);
                                    return Result<RefreshResponse>.Ok();
                                }
                            }
                            else
                            {
                                _logger.LogWarning("Refresh: Sesión {SessionId} o dispositivo {DeviceId} no coinciden. JTI: {Jti}.", currentUserSessionId, currentUserDeviceId, 
                                    currentJti);
                                await _userSessionService.BlacklistedAccessTokenSessionAsync(currentUserSessionId, currentJti, currentAccessTokenExpiration, currentUserId,
                                    "Refresh: Datos de dispositivo o sesión no encontrados.", utcNow, cancellationToken);
                                return Result<RefreshResponse>.Fail(Error.Unauthorized("Sesión no válida o dispositivo no reconocido."));
                            }
                        }
                        else
                        {
                            if (userSessionRefreshToken.UserSessionId == currentUserSessionId)
                            {
                                var mismatchedUserSession = await _userSessionRepository.GetByIdAsync(currentUserSessionId, asTracking, cancellationToken);
                                if (mismatchedUserSession is not null)
                                {
                                    _logger.LogWarning("Refresh: Fallo de hash en RT para sesión {SessionId}. Posible falsificación. JTI: {Jti}.", currentUserSessionId, 
                                        currentJti);
                                    await _userSessionService.RevokeUserSessionAndAccessTokenSessionAsync(mismatchedUserSession, currentJti, currentAccessTokenExpiration,
                                        "Refresh: Discrepancia criptográfica (Fallo de Hash).", currentUserId, currentUserSessionId, utcNow, asTracking, cancellationToken);
                                    return Result<RefreshResponse>.Fail(Error.Unauthorized("Credenciales de sesión inválidas."));
                                }
                                else
                                {
                                    _logger.LogWarning("Refresh: Fallo de hash y sesión {SessionId} inexistente. JTI: {Jti}.", currentUserSessionId, currentJti);
                                    await _userSessionService.BlacklistedAccessTokenSessionAsync(currentUserSessionId, currentJti, currentAccessTokenExpiration, currentUserId,
                                        "Refresh: Discrepancia criptográfica y sesión no encontrada.", utcNow, cancellationToken);
                                    return Result<RefreshResponse>.Fail(Error.Unauthorized("Sesión inexistente o inválida."));
                                }
                            }
                            else
                            {
                                var mismatchedUserSession = await _userSessionRepository.GetByIdAsync(userSessionRefreshToken.UserSessionId, asTracking, cancellationToken);
                                var mismatchedUserSession1 = await _userSessionRepository.GetByIdAsync(currentUserSessionId, asTracking, cancellationToken);
                                if (mismatchedUserSession is not null && mismatchedUserSession1 is not null)
                                {
                                    _logger.LogWarning("Refresh: Conflicto de sesión cruzada con fallo de hash. JTI: {Jti}.", currentJti);
                                    await _userSessionService.RevokeUserSessionAndAccessTokenSessionAsync(mismatchedUserSession, mismatchedUserSession1, currentJti,
                                        currentAccessTokenExpiration, "Refresh: Conflicto de sesión cruzada y fallo criptográfico.", currentUserId, currentUserSessionId, utcNow, 
                                        asTracking, cancellationToken);
                                    return Result<RefreshResponse>.Fail(Error.Forbidden("Inconsistencia de sesiones detectada."));
                                }
                                else if (mismatchedUserSession is not null && mismatchedUserSession1 is null)
                                {
                                    _logger.LogWarning("Refresh: Sesión cruzada. AT con sesión inexistente, RT existe pero hash inválido. JTI: {Jti}.", currentJti);
                                    await _userSessionService.RevokeUserSessionAndAccessTokenSessionAsync(mismatchedUserSession, currentJti, currentAccessTokenExpiration,
                                        "Refresh: Conflicto de sesión cruzada y AT huérfano.", currentUserId, currentUserSessionId, utcNow, asTracking, cancellationToken);
                                    return Result<RefreshResponse>.Fail(Error.Forbidden("Conflicto de sesión inválido."));
                                }
                                else if (mismatchedUserSession is null && mismatchedUserSession1 is not null)
                                {
                                    _logger.LogWarning("Refresh: Sesión cruzada. AT existe, RT inexistente con hash inválido. JTI: {Jti}.", currentJti);
                                    await _userSessionService.RevokeUserSessionAndAccessTokenSessionAsync(mismatchedUserSession1, currentJti, currentAccessTokenExpiration,
                                        "Refresh: Conflicto de sesión cruzada y RT huérfano.", currentUserId, currentUserSessionId, utcNow, asTracking, cancellationToken);
                                    return Result<RefreshResponse>.Fail(Error.Forbidden("Conflicto de sesión inválido."));
                                }
                                else
                                {
                                    _logger.LogWarning("Refresh: Sesión cruzada total. Ambas sesiones no existen y hash inválido. JTI: {Jti}.", currentJti);
                                    await _userSessionService.BlacklistedAccessTokenSessionAsync(currentUserSessionId, currentJti, currentAccessTokenExpiration, currentUserId,
                                        "Refresh: Conflicto de sesión cruzada inexistente.", utcNow, cancellationToken);
                                    return Result<RefreshResponse>.Fail(Error.Forbidden("Inconsistencia total de sesión."));
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
                                _logger.LogWarning("Refresh: Payload de RT ausente para sesión {SessionId}. JTI: {Jti}.", currentUserSessionId, currentJti);
                                await _userSessionService.RevokeUserSessionAndAccessTokenSessionAsync(mismatchedUserSession, currentJti, currentAccessTokenExpiration,
                                    "Refresh: Payload de Refresh Token ausente.", currentUserId, currentUserSessionId, utcNow, asTracking, cancellationToken);
                                return Result<RefreshResponse>.Fail(Error.Validation("Token incompleto proporcionado."));
                            }
                            else
                            {
                                _logger.LogWarning("Refresh: Payload de RT ausente y sesión {SessionId} inexistente. JTI: {Jti}.", currentUserSessionId, currentJti);
                                await _userSessionService.BlacklistedAccessTokenSessionAsync(currentUserSessionId, currentJti, currentAccessTokenExpiration, currentUserId,
                                    "Refresh: Payload ausente y sesión no encontrada.", utcNow, cancellationToken);
                                return Result<RefreshResponse>.Fail(Error.Validation("Token incompleto y sesión no válida."));
                            }
                        }
                        else
                        {
                            var mismatchedUserSession = await _userSessionRepository.GetByIdAsync(userSessionRefreshToken.UserSessionId, asTracking, cancellationToken);
                            var mismatchedUserSession1 = await _userSessionRepository.GetByIdAsync(currentUserSessionId, asTracking, cancellationToken);
                            if (mismatchedUserSession is not null && mismatchedUserSession1 is not null)
                            {
                                _logger.LogWarning("Refresh: Conflicto de sesión cruzada sin payload en RT. JTI: {Jti}.", currentJti);
                                await _userSessionService.RevokeUserSessionAndAccessTokenSessionAsync(mismatchedUserSession, mismatchedUserSession1, currentJti,
                                    currentAccessTokenExpiration, "Refresh: Conflicto cruzado sin Payload RT.", currentUserId, currentUserSessionId, utcNow, asTracking, 
                                    cancellationToken);
                                return Result<RefreshResponse>.Fail(Error.Forbidden("Conflicto estructural de sesiones."));
                            }
                            else if (mismatchedUserSession is not null && mismatchedUserSession1 is null)
                            {
                                _logger.LogWarning("Refresh: Conflicto cruzado sin payload. AT huérfano. JTI: {Jti}.", currentJti);
                                await _userSessionService.RevokeUserSessionAndAccessTokenSessionAsync(mismatchedUserSession, currentJti, currentAccessTokenExpiration,
                                    "Refresh: Conflicto cruzado sin Payload. AT huérfano.", currentUserId, currentUserSessionId, utcNow, asTracking, cancellationToken);
                                return Result<RefreshResponse>.Fail(Error.Forbidden("Estructura de sesión inválida."));
                            }
                            else if (mismatchedUserSession is null && mismatchedUserSession1 is not null)
                            {
                                _logger.LogWarning("Refresh: Conflicto cruzado sin payload. RT huérfano. JTI: {Jti}.", currentJti);
                                await _userSessionService.RevokeUserSessionAndAccessTokenSessionAsync(mismatchedUserSession1, currentJti, currentAccessTokenExpiration,
                                    "Refresh: Conflicto cruzado sin Payload. RT huérfano.", currentUserId, currentUserSessionId, utcNow, asTracking, cancellationToken);
                                return Result<RefreshResponse>.Fail(Error.Forbidden("Estructura de sesión inválida."));
                            }
                            else
                            {
                                _logger.LogWarning("Refresh: Conflicto cruzado sin payload, sesiones inexistentes. JTI: {Jti}.", currentJti);
                                await _userSessionService.BlacklistedAccessTokenSessionAsync(currentUserSessionId, currentJti, currentAccessTokenExpiration, currentUserId,
                                    "Refresh: Conflicto cruzado sin Payload y sin sesiones.", utcNow, cancellationToken);
                                return Result<RefreshResponse>.Fail(Error.Forbidden("Datos de sesión inexistentes."));
                            }
                        }
                    }
                }
                else
                {
                    var mismatchedUserSession = await _userSessionRepository.GetByIdAsync(currentUserSessionId, asTracking, cancellationToken);
                    if (mismatchedUserSession is not null)
                    {
                        _logger.LogWarning("Refresh: RT no registrado en BD para sesión {SessionId}. JTI: {Jti}.", currentUserSessionId, currentJti);
                        await _userSessionService.RevokeUserSessionAndAccessTokenSessionAsync(mismatchedUserSession, currentJti, currentAccessTokenExpiration,
                            "Refresh: Identificador de Refresh Token no registrado.", currentUserId, currentUserSessionId, utcNow, asTracking, cancellationToken);
                        return Result<RefreshResponse>.Fail(Error.Unauthorized("Token no reconocido por el sistema."));
                    }
                    else
                    {
                        _logger.LogWarning("Refresh: RT no registrado y sesión {SessionId} inexistente. JTI: {Jti}.", currentUserSessionId, currentJti);
                        await _userSessionService.BlacklistedAccessTokenSessionAsync(currentUserSessionId, currentJti, currentAccessTokenExpiration, currentUserId,
                            "Refresh: RT y sesión no registrados.", utcNow, cancellationToken);
                        return Result<RefreshResponse>.Fail(Error.Unauthorized("Credenciales no reconocidas."));
                    }
                }
            }
            else
            {
                var mismatchedUserSession = await _userSessionRepository.GetByIdAsync(currentUserSessionId, asTracking, cancellationToken);
                if (mismatchedUserSession is not null)
                {
                    _logger.LogWarning("Refresh: Identificador de RT malformado para sesión {SessionId}. JTI: {Jti}.", currentUserSessionId, currentJti);
                    await _userSessionService.RevokeUserSessionAndAccessTokenSessionAsync(mismatchedUserSession, currentJti, currentAccessTokenExpiration,
                        "Refresh: Refresh Token malformado.", currentUserId, currentUserSessionId, utcNow, asTracking, cancellationToken);
                    return Result<RefreshResponse>.Fail(Error.Validation("Formato de token erróneo."));
                }
                else
                {
                    _logger.LogWarning("Refresh: RT malformado y sesión {SessionId} inexistente. JTI: {Jti}.", currentUserSessionId, currentJti);
                    await _userSessionService.BlacklistedAccessTokenSessionAsync(currentUserSessionId, currentJti, currentAccessTokenExpiration, currentUserId, 
                        "Refresh: RT malformado y sesión inexistente.", utcNow, cancellationToken);
                    return Result<RefreshResponse>.Fail(Error.Validation("Token erróneo y sesión inválida."));
                }
            }
        }

        //public async Task<Result<RefreshResponse>> Handle(RefreshCommand request, CancellationToken cancellationToken)
        //{
        //    var validation = await _validator.ValidateAsync(request, cancellationToken);
        //    if (!validation.IsValid)
        //    {
        //        _logger.LogWarning("RefreshCommand: validación fallida. Errores: {Errors}", string.Join(" | ", validation.Errors.Select(e => e.ErrorMessage)));
        //        return Result<RefreshResponse>.Fail(validation.ToDomainErrors());
        //    }




        //    var asTracking = false;



        //    var utcNow = _dateTimeService.UtcNow;
        //    RefreshTokenParser.TryParse(request.RefreshTokenRaw, out var identifier, out var refreshTokenPlain);
        //    var userSessionRefreshToken = identifier is not null ? await _userSessionRefreshTokenRepository.GetByIdentifierAsync(identifier, asTracking, cancellationToken) : null;
        //    int currentUserId;
        //    string? currentJti = null;
        //    DateTime? currentAccessTokenExpiration = null;
        //    UserSession? userSession;
        //    string revokedReasonPrefix;
        //    if (_currentUserService.IsAuthenticated)
        //    {
        //        currentUserId = _currentUserService.UserId!.Value;
        //        var currentUserSessionId = _currentUserService.UserSessionId!.Value;
        //        var currentUserDeviceId = _currentUserService.UserDeviceId!.Value;
        //        currentJti = _currentUserService.Jti;
        //        currentAccessTokenExpiration = _currentUserService.AccessTokenExpiration;
        //        revokedReasonPrefix = "Refresh (Autenticado):";
        //        if (userSessionRefreshToken is null)
        //        {
        //            var currentAccessTokenType = _currentUserService.AccessTokenType!.Value;
        //            if (currentAccessTokenType == AccessTokenType.Temporary)
        //            {
        //                var blacklistedAccessTokenTemporary = await _blacklistedTokenService.TryGenerateBlacklistedAccessTokenTemporaryAsync(currentUserId, currentJti!,
        //                    currentAccessTokenExpiration!.Value, utcNow, revokedReasonPrefix + " Access token temporal revocado.", currentUserId, utcNow, asTracking,
        //                    cancellationToken);
        //                if (blacklistedAccessTokenTemporary is not null)
        //                {
        //                    await _unitOfWork.BlacklistedAccessTokensTemporary.InsertAsync(blacklistedAccessTokenTemporary, cancellationToken);
        //                    await _unitOfWork.SaveChangesAsync(cancellationToken);
        //                }
        //                return Result<RefreshResponse>.Fail(Error.Unauthorized("No autorizado para refresh con el token actual."));
        //            }
        //            var targetUserSession = await _userSessionRepository.GetByIdAndUserIdAndUserDeviceIdAsync(currentUserSessionId, currentUserId, currentUserDeviceId, asTracking,
        //                cancellationToken);
        //            if (targetUserSession is not null)
        //            {
        //                _logger.LogWarning("RefreshCommand: refresh token no encontrado para sesión autenticada, revocando sesión. UserId={UserId}, UserSessionId={UserSessionId}",
        //                    currentUserId, currentUserSessionId);
        //                await _userSessionService.RevokeUserSessionAsync(targetUserSession, currentJti, currentAccessTokenExpiration,
        //                    revokedReasonPrefix + " Refresh token no encontrado para esta sesión; se revoca la sesión actual por seguridad.",
        //                    currentUserId, utcNow, asTracking, cancellationToken);
        //                return Result<RefreshResponse>.Fail(Error.Unauthorized("Refresh inválido, la sesión actual ha sido revocada por seguridad."));
        //            }
        //            return Result<RefreshResponse>.Fail(Error.Unauthorized("No se encontró una sesión activa para realizar refresh."));
        //        }
        //        userSession = await _userSessionRepository.GetByIdAsync(userSessionRefreshToken.UserSessionId, asTracking, cancellationToken);
        //        if (userSession is null || userSession.Id != currentUserSessionId || userSession.UserId != currentUserId || userSession.UserDeviceId != currentUserDeviceId)
        //        {
        //            _logger.LogWarning("RefreshCommand: discrepancia entre sesión y refresh token en solicitud autenticada. UserId={UserId}, " +
        //                "IdentificadorProvisto={Identifier}, UserSessionIdDelToken={TokenUserSessionId}, UserSessionIdActual={CurrentUserSessionId}", currentUserId, identifier!,
        //                userSessionRefreshToken.UserSessionId, currentUserSessionId);
        //            var targetUserSession = await _userSessionRepository.GetByIdAndUserIdAndUserDeviceIdAsync(currentUserSessionId, currentUserId, currentUserDeviceId, asTracking,
        //                cancellationToken);
        //            if (targetUserSession is not null)
        //            {
        //                _logger.LogWarning("RefreshCommand: revocando sesión actual por discrepancia. UserId={UserId}, UserSessionId={UserSessionId}", currentUserId,
        //                    currentUserSessionId);
        //                await _userSessionService.RevokeUserSessionAsync(targetUserSession, currentJti, currentAccessTokenExpiration,
        //                    revokedReasonPrefix + " Refresh token o sesión no coincidentes; se revoca la sesión actual por seguridad.", currentUserId, utcNow, asTracking,
        //                    cancellationToken);
        //                return Result<RefreshResponse>.Fail(Error.Unauthorized("Refresh inválido, se ha revocado la sesión actual por seguridad."));
        //            }
        //            return Result<RefreshResponse>.Fail(Error.Unauthorized("No se encontró una sesión válida para el token proporcionado."));
        //        }
        //    }
        //    else
        //    {
        //        if (userSessionRefreshToken is null)
        //        {
        //            _logger.LogWarning("RefreshCommand (No Autenticado): no se proporcionó identificador de token.");
        //            return Result<RefreshResponse>.Fail(Error.Unauthorized("Token de refresh no provisto o inválido."));
        //        }
        //        userSession = await _userSessionRepository.GetByIdAsync(userSessionRefreshToken.UserSessionId, asTracking, cancellationToken);
        //        if (userSession is null)
        //        {
        //            _logger.LogWarning("RefreshCommand (No Autenticado): identificador de token provisto pero sesión no encontrada. Identificador={Identifier}", identifier!);
        //            return Result<RefreshResponse>.Fail(Error.Unauthorized("Token inválido o sesión asociada no encontrada."));
        //        }
        //        currentUserId = userSession.UserId;
        //        revokedReasonPrefix = "Refresh (No Autenticado):";
        //    }
        //    var tokenSessionValidationResult = await _tokenSessionValidationService.ValidateAsync(userSession, userSessionRefreshToken!, refreshTokenPlain, 
        //        request.DeviceIdentifier, revokedReasonPrefix, _currentUserService.IsAuthenticated, 
        //        currentUserId, _currentUserService.IsAuthenticated ? _currentUserService.WorkProfileId : null, 
        //        _currentUserService.IsAuthenticated ? _currentUserService.RoleId : null, _currentUserService.IsAuthenticated ? _currentUserService.CampusId : null,
        //        _currentUserService.IsAuthenticated ? _currentUserService.TokenVersion : null, _currentUserService.IsAuthenticated ? _currentUserService.SecurityStamp : null,
        //        utcNow, asTracking, cancellationToken);
        //    if (!tokenSessionValidationResult.IsValid)
        //    {
        //        await _userSessionService.RevokeUserSessionAsync(userSession, currentJti, currentAccessTokenExpiration, tokenSessionValidationResult.RevokedReason, currentUserId, 
        //            utcNow, asTracking, cancellationToken);
        //        return Result<RefreshResponse>.Fail(Error.Unauthorized("Refresh inválido. La sesión ha sido revocada por motivos de seguridad. Inicie sesión nuevamente."));
        //    }
        //    return await ExecuteTokenRotationAsync(userSession, userSessionRefreshToken!, tokenSessionValidationResult.ValidatedUser!, tokenSessionValidationResult.RevokedReason, 
        //        currentUserId, utcNow, currentJti, currentAccessTokenExpiration, tokenSessionValidationResult.WorkProfileId, tokenSessionValidationResult.RoleId, 
        //        tokenSessionValidationResult.CampusId, asTracking, cancellationToken);
        //}

        //private async Task<Result<RefreshResponse>> ExecuteTokenRotationAsync(UserSession userSession, UserSessionRefreshToken userSessionRefreshToken, User user, 
        //    string revokedReason, int currentUserId, DateTime utcNow, string? currentJti, DateTime? currentAccessTokenExpiration, int workProfileId, int? roleId = null, 
        //    int? campusId = null, bool asTracking = false, CancellationToken cancellationToken = default)
        //{
        //    var rotatedRefreshToken = await _userSessionService.RotateUserSessionAsync(userSession, userSessionRefreshToken, currentJti, currentAccessTokenExpiration, 
        //        revokedReason, currentUserId, utcNow, asTracking, cancellationToken);
        //    var accessToken = _tokenService.GenerateSessionAccessToken(user.Id, user.SecurityStamp, user.TokenVersion, userSession.UserDeviceId, userSession.Id, utcNow,
        //        _tokenLifetimeSettings.SessionAccessTokenLifetimeMinutes, workProfileId, roleId, campusId);
        //    return Result<RefreshResponse>.Ok(new RefreshResponse(accessToken.Token, accessToken.ExpiresIn, accessToken.ExpiresAt, rotatedRefreshToken.Identifier,
        //        rotatedRefreshToken.Token, rotatedRefreshToken.ExpiresIn, rotatedRefreshToken.ExpiresAt));
        //}
    }
}