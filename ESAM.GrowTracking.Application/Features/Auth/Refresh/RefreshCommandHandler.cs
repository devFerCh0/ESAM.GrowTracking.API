using ESAM.GrowTracking.Application.Abstractions.Services;
using ESAM.GrowTracking.Application.Enums;
using ESAM.GrowTracking.Application.Extensions;
using ESAM.GrowTracking.Application.Results;
using ESAM.GrowTracking.Application.Settings;
using ESAM.GrowTracking.Application.ValueObjects;
using ESAM.GrowTracking.Domain.Entities;
using ESAM.GrowTracking.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ESAM.GrowTracking.Application.Features.Auth.Refresh
{
    public class RefreshCommandHandler : IRequestHandler<RefreshCommand, Result<RefreshResponse>>
    {
        private readonly ILogger<RefreshCommandHandler> _logger;
        private readonly IValidator<RefreshCommand> _validator;
        private readonly IAccessTokenClaimsValidatorService _accessTokenClaimsValidatorService;
        private readonly IDateTimeService _dateTimeService;
        private readonly IUserSessionService _userSessionService;
        private readonly IAuthSessionIntegrityValidatorService _authSessionIntegrityValidatorService;
        private readonly ISecurityValidatorService _securityValidatorService;
        private readonly ITokenService _tokenService;
        private readonly TokenLifetimeSettings _tokenLifetimeSettings;

        public RefreshCommandHandler(ILogger<RefreshCommandHandler> logger, IValidator<RefreshCommand> validator, 
            IAccessTokenClaimsValidatorService accessTokenClaimsValidatorService, IDateTimeService dateTimeService, IUserSessionService userSessionService,
            IAuthSessionIntegrityValidatorService authSessionIntegrityValidatorService, ISecurityValidatorService securityValidatorService, ITokenService tokenService, 
            IOptions<TokenLifetimeSettings> tokenLifetimeSettingsOptions)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(validator);
            ArgumentNullException.ThrowIfNull(accessTokenClaimsValidatorService);
            ArgumentNullException.ThrowIfNull(dateTimeService);
            ArgumentNullException.ThrowIfNull(userSessionService);
            ArgumentNullException.ThrowIfNull(authSessionIntegrityValidatorService);
            ArgumentNullException.ThrowIfNull(securityValidatorService);
            ArgumentNullException.ThrowIfNull(tokenService);
            ArgumentNullException.ThrowIfNull(tokenLifetimeSettingsOptions);
            _logger = logger;
            _validator = validator;
            _accessTokenClaimsValidatorService = accessTokenClaimsValidatorService;
            _dateTimeService = dateTimeService;
            _userSessionService = userSessionService;
            _authSessionIntegrityValidatorService = authSessionIntegrityValidatorService;
            _securityValidatorService = securityValidatorService;
            _tokenService = tokenService;
            _tokenLifetimeSettings = tokenLifetimeSettingsOptions.Value ?? throw new ArgumentNullException(nameof(tokenLifetimeSettingsOptions));
        }

        public async Task<Result<RefreshResponse>> Handle(RefreshCommand request, CancellationToken cancellationToken)
        {
            var validation = await _validator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                _logger.LogWarning("RefreshCommand: Validación fallida. Errores: {Errors}", string.Join(" | ", validation.Errors.Select(e => e.ErrorMessage)));
                return Result<RefreshResponse>.Fail(validation.ToCommandErrors());
            }
            await _accessTokenClaimsValidatorService.UseExplicitAccessTokenAsync(request.AccessToken);
            var asTracking = false;
            var currentJti = _accessTokenClaimsValidatorService.CurrentJti;
            var currentUserId = _accessTokenClaimsValidatorService.CurrentUserId;
            var currentAccessTokenExpiration = _accessTokenClaimsValidatorService.CurrentAccessTokenExpiration;
            var utcNow = _dateTimeService.UtcNow;
            if (_accessTokenClaimsValidatorService.CurrentAccessTokenType == AccessTokenType.Temporary)
            {
                var deniedResult = await _authSessionIntegrityValidatorService.ValidateTemporaryAccessTokenAsync("Refresh", request.RefreshTokenRaw, currentJti,
                    currentAccessTokenExpiration, currentUserId, utcNow, asTracking, cancellationToken);
                return Result<RefreshResponse>.Fail(deniedResult.Error!);
            }
            var currentUserSessionId = _accessTokenClaimsValidatorService.CurrentUserSessionId;
            var currentUserDeviceId = _accessTokenClaimsValidatorService.CurrentUserDeviceId;
            var integrityResult = await _authSessionIntegrityValidatorService.ValidateSessionAccessTokenAsync("Refresh", request.RefreshTokenRaw, currentJti,
                currentAccessTokenExpiration, currentUserId, currentUserSessionId, currentUserDeviceId, utcNow, asTracking, cancellationToken);
            if (!integrityResult.IsValid)
                return Result<RefreshResponse>.Fail(integrityResult.Error!);
            return await FinalizeSuccessfulRefreshAsync(integrityResult.UserSession!, integrityResult.UserSessionRefreshToken!, currentJti, currentAccessTokenExpiration, 
                currentUserId, currentUserSessionId, currentUserDeviceId, utcNow, asTracking, cancellationToken);
        }

        private async Task<Result<RefreshResponse>> FinalizeSuccessfulRefreshAsync(UserSession userSession, UserSessionRefreshToken userSessionRefreshToken, string currentJti, 
            DateTime currentAccessTokenExpiration, int currentUserId, int currentUserSessionId, int currentUserDeviceId, DateTime utcNow, bool asTracking, 
            CancellationToken cancellationToken)
        {
            var currentSecurityStamp = _accessTokenClaimsValidatorService.CurrentSecurityStamp;
            var currentTokenVersion = _accessTokenClaimsValidatorService.CurrentTokenVersion;
            var currentWorkProfileId = _accessTokenClaimsValidatorService.CurrentWorkProfileId;
            var currentWorkProfileType = _accessTokenClaimsValidatorService.CurrentWorkProfileType;
            var currentRoleId = _accessTokenClaimsValidatorService.CurrentRoleId;
            var currentCampusId = _accessTokenClaimsValidatorService.CurrentCampusId;
            var isOnlyWorkProfile = currentWorkProfileType == WorkProfileType.OnlyWorkProfile;
            var securityValidationResult = isOnlyWorkProfile
                ? await _securityValidatorService.ValidateAccessTokenSessionAsync(currentJti, currentUserId, currentSecurityStamp, currentTokenVersion, currentUserDeviceId, 
                    currentUserSessionId, currentWorkProfileId, currentWorkProfileType, cancellationToken)
                : await _securityValidatorService.ValidateAccessTokenSessionAsync(currentJti, currentUserId, currentSecurityStamp, currentTokenVersion, currentUserDeviceId, 
                    currentUserSessionId, currentWorkProfileId, currentWorkProfileType, currentRoleId, currentCampusId, cancellationToken);
            if (securityValidationResult.IsFailure)
            {
                var reason = isOnlyWorkProfile
                    ? "Refresh: Estado de usuario o contexto de seguridad alterado (SecurityStamp/TokenVersion/Dispositivo)."
                    : "Refresh: Contexto de seguridad alterado o revocación de permisos (Rol/Campus/SecurityStamp).";
                var errorMessage = isOnlyWorkProfile
                    ? "Tu sesión ha expirado por un cambio en la seguridad de la cuenta. Inicia sesión nuevamente."
                    : "Los permisos de tu cuenta o perfil han cambiado. Inicia sesión nuevamente.";
                _logger.LogWarning("Refresh: Invalidación de seguridad para sesión {SessionId}. JTI: {Jti}. Motivo: {Reason}", currentUserSessionId, currentJti, reason);
                await _userSessionService.RevokeUserSessionAndAccessTokenSessionAsync(userSession, currentJti, currentAccessTokenExpiration, reason, currentUserId,
                    currentUserSessionId, utcNow, asTracking, cancellationToken);
                return Result<RefreshResponse>.Fail(Error.Unauthorized(errorMessage));
            }
            _logger.LogInformation("Refresh: Validación exitosa para sesión {SessionId}. Procediendo con rotación. JTI: {Jti}.", currentUserSessionId, currentJti);
            var refreshToken = await _userSessionService.RotateUserSessionAsync(userSession, userSessionRefreshToken, currentJti, currentAccessTokenExpiration,
                "Refresh: Token reemplazado exitosamente por rotación normal.", currentUserId, utcNow, asTracking, cancellationToken);
            var accessToken = isOnlyWorkProfile
                ? _tokenService.GenerateSessionAccessToken(currentUserId, currentSecurityStamp, currentTokenVersion, currentUserDeviceId, currentUserSessionId, utcNow, 
                    _tokenLifetimeSettings.SessionAccessTokenLifetimeMinutes, currentWorkProfileId, currentWorkProfileType)
                : _tokenService.GenerateSessionAccessToken(currentUserId, currentSecurityStamp, currentTokenVersion, currentUserDeviceId, currentUserSessionId, utcNow, 
                    _tokenLifetimeSettings.SessionAccessTokenLifetimeMinutes, currentWorkProfileId, currentWorkProfileType, currentRoleId, currentCampusId);
            return Result<RefreshResponse>.Ok(new RefreshResponse(accessToken.Token, accessToken.ExpiresIn, accessToken.ExpiresAt, refreshToken.Identifier, refreshToken.Token, 
                refreshToken.ExpiresIn, refreshToken.ExpiresAt));
        }
    }

    //public class RefreshCommandHandler : IRequestHandler<RefreshCommand, Result<RefreshResponse>>
    //{
    //    private readonly ILogger<RefreshCommandHandler> _logger;
    //    private readonly IValidator<RefreshCommand> _validator;
    //    private readonly IAccessTokenClaimsValidatorService _accessTokenClaimsValidatorService;
    //    private readonly IDateTimeService _dateTimeService;
    //    private readonly IUserSessionRefreshTokenRepository _userSessionRefreshTokenRepository;
    //    private readonly IUserSessionRepository _userSessionRepository;
    //    private readonly IUserSessionService _userSessionService;
    //    private readonly IHashService _hashService;
    //    private readonly ISecurityValidatorService _securityValidatorService;
    //    private readonly ITokenService _tokenService;
    //    private readonly TokenLifetimeSettings _tokenLifetimeSettings;

    //    public RefreshCommandHandler(ILogger<RefreshCommandHandler> logger, IValidator<RefreshCommand> validator, 
    //        IAccessTokenClaimsValidatorService accessTokenClaimsValidatorService, IDateTimeService dateTimeService, 
    //        IUserSessionRefreshTokenRepository userSessionRefreshTokenRepository, IUserSessionRepository userSessionRepository, IUserSessionService userSessionService, 
    //        IHashService hashService, ISecurityValidatorService securityValidatorService, ITokenService tokenService, IOptions<TokenLifetimeSettings> tokenLifetimeSettingsOptions)
    //    {
    //        ArgumentNullException.ThrowIfNull(logger);
    //        ArgumentNullException.ThrowIfNull(validator);
    //        ArgumentNullException.ThrowIfNull(accessTokenClaimsValidatorService);
    //        ArgumentNullException.ThrowIfNull(dateTimeService);
    //        ArgumentNullException.ThrowIfNull(userSessionRefreshTokenRepository);
    //        ArgumentNullException.ThrowIfNull(userSessionRepository);
    //        ArgumentNullException.ThrowIfNull(userSessionService);
    //        ArgumentNullException.ThrowIfNull(hashService);
    //        ArgumentNullException.ThrowIfNull(securityValidatorService);
    //        ArgumentNullException.ThrowIfNull(tokenService);
    //        ArgumentNullException.ThrowIfNull(tokenLifetimeSettingsOptions);
    //        _logger = logger;
    //        _validator = validator;
    //        _accessTokenClaimsValidatorService = accessTokenClaimsValidatorService;
    //        _dateTimeService = dateTimeService;
    //        _userSessionRefreshTokenRepository = userSessionRefreshTokenRepository;
    //        _userSessionRepository = userSessionRepository;
    //        _userSessionService = userSessionService;
    //        _hashService = hashService;
    //        _securityValidatorService = securityValidatorService;
    //        _tokenService = tokenService;
    //        _tokenLifetimeSettings = tokenLifetimeSettingsOptions.Value ?? throw new ArgumentNullException(nameof(tokenLifetimeSettingsOptions));
    //    }

    //    public async Task<Result<RefreshResponse>> Handle(RefreshCommand request, CancellationToken cancellationToken)
    //    {
    //        var validation = await _validator.ValidateAsync(request, cancellationToken);
    //        if (!validation.IsValid)
    //        {
    //            _logger.LogWarning("LogoutCommand: Validación fallida. Errores: {Errors}", string.Join(" | ", validation.Errors.Select(e => e.ErrorMessage)));
    //            return Result<RefreshResponse>.Fail(validation.ToCommandErrors());
    //        }
    //        await _accessTokenClaimsValidatorService.UseExplicitAccessTokenAsync(request.AccessToken);
    //        var currentAccessTokenType = _accessTokenClaimsValidatorService.CurrentAccessTokenType;
    //        var currentJti = _accessTokenClaimsValidatorService.CurrentJti;
    //        var currentAccessTokenExpiration = _accessTokenClaimsValidatorService.CurrentAccessTokenExpiration;
    //        var currentUserId = _accessTokenClaimsValidatorService.CurrentUserId;
    //        var utcNow = _dateTimeService.UtcNow;
    //        var asTracking = false;
    //        if (currentAccessTokenType == AccessTokenType.Temporary)
    //            return await DenyRefreshAccessTokenTemporary(request.RefreshTokenRaw, currentJti, currentAccessTokenExpiration, currentUserId, utcNow, asTracking, 
    //                cancellationToken);
    //        else
    //        {
    //            var currentUserSessionId = _accessTokenClaimsValidatorService.CurrentUserSessionId;
    //            var currentUserDeviceId = _accessTokenClaimsValidatorService.CurrentUserDeviceId;
    //            return await RefreshAccessTokenSession(request.RefreshTokenRaw, currentJti, currentAccessTokenExpiration, currentUserId, currentUserSessionId, currentUserDeviceId,
    //                utcNow, asTracking, cancellationToken);
    //        }
    //    }

    //    private async Task<Result<RefreshResponse>> DenyRefreshAccessTokenTemporary(string? refreshTokenRaw, string currentJti, DateTime currentAccessTokenExpiration, 
    //        int currentUserId, DateTime utcNow, bool asTracking = false, CancellationToken cancellationToken = default)
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
    //                        _logger.LogWarning("Refresh: Intento con Token Temporal (JTI: {Jti}) y RT válido de sesión {SessionId}.", currentJti, mismatchedUserSession.Id);
    //                        await _userSessionService.RevokeUserSessionAndAccessTokenTemporaryAsync(mismatchedUserSession, currentJti, currentAccessTokenExpiration,
    //                            "Refresh: Uso de Refresh Token con Access Token Temporal.", currentUserId, utcNow, asTracking, cancellationToken);
    //                        return Result<RefreshResponse>.Fail(Error.Forbidden("Renovación no permitida para tokens temporales. Sesión comprometida."));
    //                    }
    //                    else
    //                    {
    //                        _logger.LogWarning("Refresh: Intento con Token Temporal (JTI: {Jti}) y RT de sesión inexistente.", currentJti);
    //                        await _userSessionService.RevokeAccessTokenTemporaryAsync(currentUserId, currentJti, currentAccessTokenExpiration,
    //                            "Refresh: Uso de Refresh Token huérfano con Access Token Temporal.", utcNow, asTracking, cancellationToken);
    //                        return Result<RefreshResponse>.Fail(Error.Forbidden("Renovación denegada. Token temporal inválido."));
    //                    }
    //                }
    //                else
    //                {
    //                    _logger.LogWarning("Refresh: Intento con Token Temporal (JTI: {Jti}) y RT inexistente.", currentJti);
    //                    await _userSessionService.RevokeAccessTokenTemporaryAsync(currentUserId, currentJti, currentAccessTokenExpiration,
    //                        "Refresh: Uso de Refresh Token no registrado con Access Token Temporal.", utcNow, asTracking, cancellationToken);
    //                    return Result<RefreshResponse>.Fail(Error.Forbidden("Renovación denegada. Credenciales no registradas."));
    //                }
    //            }
    //            else
    //            {
    //                _logger.LogWarning("Refresh: Intento con Token Temporal (JTI: {Jti}) y RT malformado.", currentJti);
    //                await _userSessionService.RevokeAccessTokenTemporaryAsync(currentUserId, currentJti, currentAccessTokenExpiration,
    //                    "Refresh: Uso de Refresh Token malformado con Access Token Temporal.", utcNow, asTracking, cancellationToken);
    //                return Result<RefreshResponse>.Fail(Error.Validation("Formato de token inválido."));
    //            }
    //        }
    //        else
    //        {
    //            _logger.LogWarning("Refresh: Intento de renovar Token Temporal sin RT (JTI: {Jti}).", currentJti);
    //            await _userSessionService.RevokeAccessTokenTemporaryAsync(currentUserId, currentJti, currentAccessTokenExpiration,
    //                "Refresh: Intento de renovación de token temporal.", utcNow, asTracking, cancellationToken);
    //            return Result<RefreshResponse>.Fail(Error.Validation("Los tokens temporales no admiten renovación."));
    //        }
    //    }

    //    private async Task<Result<RefreshResponse>> RefreshAccessTokenSession(string? refreshTokenRaw, string currentJti, DateTime currentAccessTokenExpiration, int currentUserId,
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
    //                                    _logger.LogWarning("Refresh: Sesión {SessionId} alcanzó expiración absoluta. JTI: {Jti}.", currentUserSessionId, currentJti);
    //                                    await _userSessionService.RevokeUserSessionAndAccessTokenSessionAsync(userSession, currentJti, currentAccessTokenExpiration,
    //                                        "Refresh: Expiración absoluta alcanzada.", currentUserId, currentUserSessionId, utcNow, asTracking, cancellationToken);
    //                                    return Result<RefreshResponse>.Fail(Error.Unauthorized("La sesión ha expirado permanentemente."));
    //                                }
    //                                else if (userSession.ExpiresAt <= utcNow || userSessionRefreshToken.ExpiresAt <= utcNow)
    //                                {
    //                                    _logger.LogWarning("Refresh: Sesión {SessionId} o RT expirados. JTI: {Jti}.", currentUserSessionId, currentJti);
    //                                    await _userSessionService.RevokeUserSessionAndAccessTokenSessionAsync(userSession, currentJti, currentAccessTokenExpiration,
    //                                        "Refresh: Tiempo de inactividad superado.", currentUserId, currentUserSessionId, utcNow, asTracking, cancellationToken);
    //                                    return Result<RefreshResponse>.Fail(Error.Unauthorized("La sesión por inactividad ha expirado."));
    //                                }
    //                                else if (userSession.IsRevoked || userSessionRefreshToken.IsRevoked)
    //                                {
    //                                    _logger.LogWarning("Refresh: Intento sobre sesión {SessionId} o RT ya revocados. JTI: {Jti}.", currentUserSessionId, currentJti);
    //                                    await _userSessionService.RevokeUserSessionAndAccessTokenSessionAsync(userSession, currentJti, currentAccessTokenExpiration,
    //                                        "Refresh: Uso de credenciales previamente revocadas.", currentUserId, currentUserSessionId, utcNow, asTracking, cancellationToken);
    //                                    return Result<RefreshResponse>.Fail(Error.Unauthorized("La sesión ya fue terminada previamente."));
    //                                }
    //                                else if (userSessionRefreshToken.ReplacedByUserSessionRefreshTokenId.HasValue)
    //                                {
    //                                    _logger.LogWarning("Refresh: Posible ataque de repetición. RT rotado en sesión {SessionId}. JTI: {Jti}.", currentUserSessionId, currentJti);
    //                                    await _userSessionService.RevokeUserSessionAndAccessTokenSessionAsync(userSession, currentJti, currentAccessTokenExpiration,
    //                                        "Refresh: Ataque de repetición. Uso de RT reemplazado.", currentUserId, currentUserSessionId, utcNow, asTracking, cancellationToken);
    //                                    return Result<RefreshResponse>.Fail(Error.Forbidden("Detectada anomalía de seguridad. Sesión invalidada."));
    //                                }
    //                                else
    //                                {
    //                                    var currentSecurityStamp = _accessTokenClaimsValidatorService.CurrentSecurityStamp;
    //                                    var currentTokenVersion = _accessTokenClaimsValidatorService.CurrentTokenVersion;
    //                                    var currentWorkProfileId = _accessTokenClaimsValidatorService.CurrentWorkProfileId;
    //                                    var currentWorkProfileType = _accessTokenClaimsValidatorService.CurrentWorkProfileType;
    //                                    if (currentWorkProfileType == WorkProfileType.OnlyWorkProfile)
    //                                    {
    //                                        var ValidateAccessTokenSessionResult = await _securityValidatorService.ValidateAccessTokenSessionAsync(currentJti, currentUserId,
    //                                            currentSecurityStamp, currentTokenVersion, currentUserDeviceId, currentUserSessionId, currentWorkProfileId, currentWorkProfileType,
    //                                            cancellationToken);
    //                                        if (ValidateAccessTokenSessionResult.IsFailure)
    //                                        {
    //                                            _logger.LogWarning("Refresh: Invalidación de seguridad para sesión {SessionId}. El estado del usuario, SecurityStamp o " +
    //                                                "versión de token han cambiado. JTI: {Jti}.", currentUserSessionId, currentJti);
    //                                            await _userSessionService.RevokeUserSessionAndAccessTokenSessionAsync(userSession, currentJti, currentAccessTokenExpiration,
    //                                                "Refresh: Estado de usuario o contexto de seguridad alterado (SecurityStamp/TokenVersion/Dispositivo).", currentUserId,
    //                                                currentUserSessionId, utcNow, asTracking, cancellationToken);
    //                                            return Result<RefreshResponse>.Fail(Error.Unauthorized("Tu sesión ha expirado por un cambio en la seguridad de la cuenta. " +
    //                                                "Inicia sesión nuevamente."));
    //                                        }
    //                                        else
    //                                        {
    //                                            _logger.LogInformation("Refresh: Validación exitosa para sesión {SessionId}. Procediendo con rotación. JTI: {Jti}.",
    //                                                currentUserSessionId, currentJti);
    //                                            var refreshToken = await _userSessionService.RotateUserSessionAsync(userSession, userSessionRefreshToken, currentJti,
    //                                                currentAccessTokenExpiration, "Refresh: Token reemplazado exitosamente por rotación normal.", currentUserId, utcNow, asTracking,
    //                                                cancellationToken);
    //                                            var accessToken = _tokenService.GenerateSessionAccessToken(currentUserId, currentSecurityStamp, currentTokenVersion,
    //                                                currentUserDeviceId, currentUserSessionId, utcNow, _tokenLifetimeSettings.SessionAccessTokenLifetimeMinutes, 
    //                                                currentWorkProfileId, currentWorkProfileType);
    //                                            return Result<RefreshResponse>.Ok(new RefreshResponse(accessToken.Token, accessToken.ExpiresIn, accessToken.ExpiresAt,
    //                                                refreshToken.Identifier, refreshToken.Token, refreshToken.ExpiresIn, refreshToken.ExpiresAt));
    //                                        }
    //                                    }
    //                                    else
    //                                    {
    //                                        var currentRoleId = _accessTokenClaimsValidatorService.CurrentRoleId;
    //                                        var currentCampusId = _accessTokenClaimsValidatorService.CurrentCampusId;
    //                                        var ValidateAccessTokenSessionResult = await _securityValidatorService.ValidateAccessTokenSessionAsync(currentJti, currentUserId,
    //                                            currentSecurityStamp, currentTokenVersion, currentUserDeviceId, currentUserSessionId, currentWorkProfileId, currentWorkProfileType,
    //                                            currentRoleId, currentCampusId, cancellationToken);
    //                                        if (ValidateAccessTokenSessionResult.IsFailure)
    //                                        {
    //                                            _logger.LogWarning("Refresh: Invalidación de seguridad/permisos para sesión {SessionId}. Cambios detectados en estado, " +
    //                                                "SecurityStamp, Rol o Campus. JTI: {Jti}.", currentUserSessionId, currentJti);
    //                                            await _userSessionService.RevokeUserSessionAndAccessTokenSessionAsync(userSession, currentJti, currentAccessTokenExpiration,
    //                                                "Refresh: Contexto de seguridad alterado o revocación de permisos (Rol/Campus/SecurityStamp).", currentUserId, 
    //                                                currentUserSessionId, utcNow, asTracking, cancellationToken);
    //                                            return Result<RefreshResponse>.Fail(Error.Unauthorized("Los permisos de tu cuenta o perfil han cambiado. " + 
    //                                                "Inicia sesión nuevamente."));
    //                                        }
    //                                        else
    //                                        {
    //                                            _logger.LogInformation("Refresh: Validación exitosa para sesión {SessionId}. Procediendo con rotación. JTI: {Jti}.",
    //                                                currentUserSessionId, currentJti);
    //                                            var refreshToken = await _userSessionService.RotateUserSessionAsync(userSession, userSessionRefreshToken, currentJti,
    //                                                currentAccessTokenExpiration, "Refresh: Token reemplazado exitosamente por rotación normal.", currentUserId, utcNow, asTracking,
    //                                                cancellationToken);
    //                                            var accessToken = _tokenService.GenerateSessionAccessToken(currentUserId, currentSecurityStamp, currentTokenVersion,
    //                                                currentUserDeviceId, currentUserSessionId, utcNow, _tokenLifetimeSettings.SessionAccessTokenLifetimeMinutes, 
    //                                                currentWorkProfileId, currentWorkProfileType, currentRoleId, currentCampusId);
    //                                            return Result<RefreshResponse>.Ok(new RefreshResponse(accessToken.Token, accessToken.ExpiresIn, accessToken.ExpiresAt,
    //                                                refreshToken.Identifier, refreshToken.Token, refreshToken.ExpiresIn, refreshToken.ExpiresAt));
    //                                        }
    //                                    }
    //                                }
    //                            }
    //                            else
    //                            {
    //                                _logger.LogWarning("Refresh: Sesión {SessionId} o dispositivo {DeviceId} no coinciden. JTI: {Jti}.", currentUserSessionId, currentUserDeviceId,
    //                                    currentJti);
    //                                await _userSessionService.RevokeAccessTokenSessionAsync(currentUserSessionId, currentJti, currentAccessTokenExpiration, currentUserId,
    //                                    "Refresh: Datos de dispositivo o sesión no encontrados.", utcNow, asTracking, cancellationToken);
    //                                return Result<RefreshResponse>.Fail(Error.Unauthorized("Sesión no válida o dispositivo no reconocido."));
    //                            }
    //                        }
    //                        else
    //                        {
    //                            if (userSessionRefreshToken.UserSessionId == currentUserSessionId)
    //                            {
    //                                var mismatchedUserSession = await _userSessionRepository.GetByIdAsync(currentUserSessionId, asTracking, cancellationToken);
    //                                if (mismatchedUserSession is not null)
    //                                {
    //                                    _logger.LogWarning("Refresh: Fallo de hash en RT para sesión {SessionId}. Posible falsificación. JTI: {Jti}.", currentUserSessionId,
    //                                        currentJti);
    //                                    await _userSessionService.RevokeUserSessionAndAccessTokenSessionAsync(mismatchedUserSession, currentJti, currentAccessTokenExpiration,
    //                                        "Refresh: Discrepancia criptográfica (Fallo de Hash).", currentUserId, currentUserSessionId, utcNow, asTracking, cancellationToken);
    //                                    return Result<RefreshResponse>.Fail(Error.Unauthorized("Credenciales de sesión inválidas."));
    //                                }
    //                                else
    //                                {
    //                                    _logger.LogWarning("Refresh: Fallo de hash y sesión {SessionId} inexistente. JTI: {Jti}.", currentUserSessionId, currentJti);
    //                                    await _userSessionService.RevokeAccessTokenSessionAsync(currentUserSessionId, currentJti, currentAccessTokenExpiration, currentUserId,
    //                                        "Refresh: Discrepancia criptográfica y sesión no encontrada.", utcNow, asTracking, cancellationToken);
    //                                    return Result<RefreshResponse>.Fail(Error.Unauthorized("Sesión inexistente o inválida."));
    //                                }
    //                            }
    //                            else
    //                            {
    //                                var mismatchedUserSession = await _userSessionRepository.GetByIdAsync(userSessionRefreshToken.UserSessionId, asTracking, cancellationToken);
    //                                var mismatchedUserSession1 = await _userSessionRepository.GetByIdAsync(currentUserSessionId, asTracking, cancellationToken);
    //                                if (mismatchedUserSession is not null && mismatchedUserSession1 is not null)
    //                                {
    //                                    _logger.LogWarning("Refresh: Conflicto de sesión cruzada con fallo de hash. JTI: {Jti}.", currentJti);
    //                                    await _userSessionService.RevokeUserSessionAndAccessTokenSessionAsync(mismatchedUserSession, mismatchedUserSession1, currentJti,
    //                                        currentAccessTokenExpiration, "Refresh: Conflicto de sesión cruzada y fallo criptográfico.", currentUserId, currentUserSessionId, 
    //                                        utcNow, asTracking, cancellationToken);
    //                                    return Result<RefreshResponse>.Fail(Error.Forbidden("Inconsistencia de sesiones detectada."));
    //                                }
    //                                else if (mismatchedUserSession is not null && mismatchedUserSession1 is null)
    //                                {
    //                                    _logger.LogWarning("Refresh: Sesión cruzada. AT con sesión inexistente, RT existe pero hash inválido. JTI: {Jti}.", currentJti);
    //                                    await _userSessionService.RevokeUserSessionAndAccessTokenSessionAsync(mismatchedUserSession, currentJti, currentAccessTokenExpiration,
    //                                        "Refresh: Conflicto de sesión cruzada y AT huérfano.", currentUserId, currentUserSessionId, utcNow, asTracking, cancellationToken);
    //                                    return Result<RefreshResponse>.Fail(Error.Forbidden("Conflicto de sesión inválido."));
    //                                }
    //                                else if (mismatchedUserSession is null && mismatchedUserSession1 is not null)
    //                                {
    //                                    _logger.LogWarning("Refresh: Sesión cruzada. AT existe, RT inexistente con hash inválido. JTI: {Jti}.", currentJti);
    //                                    await _userSessionService.RevokeUserSessionAndAccessTokenSessionAsync(mismatchedUserSession1, currentJti, currentAccessTokenExpiration,
    //                                        "Refresh: Conflicto de sesión cruzada y RT huérfano.", currentUserId, currentUserSessionId, utcNow, asTracking, cancellationToken);
    //                                    return Result<RefreshResponse>.Fail(Error.Forbidden("Conflicto de sesión inválido."));
    //                                }
    //                                else
    //                                {
    //                                    _logger.LogWarning("Refresh: Sesión cruzada total. Ambas sesiones no existen y hash inválido. JTI: {Jti}.", currentJti);
    //                                    await _userSessionService.RevokeAccessTokenSessionAsync(currentUserSessionId, currentJti, currentAccessTokenExpiration, currentUserId,
    //                                        "Refresh: Conflicto de sesión cruzada inexistente.", utcNow, asTracking, cancellationToken);
    //                                    return Result<RefreshResponse>.Fail(Error.Forbidden("Inconsistencia total de sesión."));
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
    //                                _logger.LogWarning("Refresh: Payload de RT ausente para sesión {SessionId}. JTI: {Jti}.", currentUserSessionId, currentJti);
    //                                await _userSessionService.RevokeUserSessionAndAccessTokenSessionAsync(mismatchedUserSession, currentJti, currentAccessTokenExpiration,
    //                                    "Refresh: Payload de Refresh Token ausente.", currentUserId, currentUserSessionId, utcNow, asTracking, cancellationToken);
    //                                return Result<RefreshResponse>.Fail(Error.Validation("Token incompleto proporcionado."));
    //                            }
    //                            else
    //                            {
    //                                _logger.LogWarning("Refresh: Payload de RT ausente y sesión {SessionId} inexistente. JTI: {Jti}.", currentUserSessionId, currentJti);
    //                                await _userSessionService.RevokeAccessTokenSessionAsync(currentUserSessionId, currentJti, currentAccessTokenExpiration, currentUserId,
    //                                    "Refresh: Payload ausente y sesión no encontrada.", utcNow, asTracking, cancellationToken);
    //                                return Result<RefreshResponse>.Fail(Error.Validation("Token incompleto y sesión no válida."));
    //                            }
    //                        }
    //                        else
    //                        {
    //                            var mismatchedUserSession = await _userSessionRepository.GetByIdAsync(userSessionRefreshToken.UserSessionId, asTracking, cancellationToken);
    //                            var mismatchedUserSession1 = await _userSessionRepository.GetByIdAsync(currentUserSessionId, asTracking, cancellationToken);
    //                            if (mismatchedUserSession is not null && mismatchedUserSession1 is not null)
    //                            {
    //                                _logger.LogWarning("Refresh: Conflicto de sesión cruzada sin payload en RT. JTI: {Jti}.", currentJti);
    //                                await _userSessionService.RevokeUserSessionAndAccessTokenSessionAsync(mismatchedUserSession, mismatchedUserSession1, currentJti,
    //                                    currentAccessTokenExpiration, "Refresh: Conflicto cruzado sin Payload RT.", currentUserId, currentUserSessionId, utcNow, asTracking,
    //                                    cancellationToken);
    //                                return Result<RefreshResponse>.Fail(Error.Forbidden("Conflicto estructural de sesiones."));
    //                            }
    //                            else if (mismatchedUserSession is not null && mismatchedUserSession1 is null)
    //                            {
    //                                _logger.LogWarning("Refresh: Conflicto cruzado sin payload. AT huérfano. JTI: {Jti}.", currentJti);
    //                                await _userSessionService.RevokeUserSessionAndAccessTokenSessionAsync(mismatchedUserSession, currentJti, currentAccessTokenExpiration,
    //                                    "Refresh: Conflicto cruzado sin Payload. AT huérfano.", currentUserId, currentUserSessionId, utcNow, asTracking, cancellationToken);
    //                                return Result<RefreshResponse>.Fail(Error.Forbidden("Estructura de sesión inválida."));
    //                            }
    //                            else if (mismatchedUserSession is null && mismatchedUserSession1 is not null)
    //                            {
    //                                _logger.LogWarning("Refresh: Conflicto cruzado sin payload. RT huérfano. JTI: {Jti}.", currentJti);
    //                                await _userSessionService.RevokeUserSessionAndAccessTokenSessionAsync(mismatchedUserSession1, currentJti, currentAccessTokenExpiration,
    //                                    "Refresh: Conflicto cruzado sin Payload. RT huérfano.", currentUserId, currentUserSessionId, utcNow, asTracking, cancellationToken);
    //                                return Result<RefreshResponse>.Fail(Error.Forbidden("Estructura de sesión inválida."));
    //                            }
    //                            else
    //                            {
    //                                _logger.LogWarning("Refresh: Conflicto cruzado sin payload, sesiones inexistentes. JTI: {Jti}.", currentJti);
    //                                await _userSessionService.RevokeAccessTokenSessionAsync(currentUserSessionId, currentJti, currentAccessTokenExpiration, currentUserId,
    //                                    "Refresh: Conflicto cruzado sin Payload y sin sesiones.", utcNow, asTracking, cancellationToken);
    //                                return Result<RefreshResponse>.Fail(Error.Forbidden("Datos de sesión inexistentes."));
    //                            }
    //                        }
    //                    }
    //                }
    //                else
    //                {
    //                    var mismatchedUserSession = await _userSessionRepository.GetByIdAsync(currentUserSessionId, asTracking, cancellationToken);
    //                    if (mismatchedUserSession is not null)
    //                    {
    //                        _logger.LogWarning("Refresh: RT no registrado en BD para sesión {SessionId}. JTI: {Jti}.", currentUserSessionId, currentJti);
    //                        await _userSessionService.RevokeUserSessionAndAccessTokenSessionAsync(mismatchedUserSession, currentJti, currentAccessTokenExpiration,
    //                            "Refresh: Identificador de Refresh Token no registrado.", currentUserId, currentUserSessionId, utcNow, asTracking, cancellationToken);
    //                        return Result<RefreshResponse>.Fail(Error.Unauthorized("Token no reconocido por el sistema."));
    //                    }
    //                    else
    //                    {
    //                        _logger.LogWarning("Refresh: RT no registrado y sesión {SessionId} inexistente. JTI: {Jti}.", currentUserSessionId, currentJti);
    //                        await _userSessionService.RevokeAccessTokenSessionAsync(currentUserSessionId, currentJti, currentAccessTokenExpiration, currentUserId,
    //                            "Refresh: RT y sesión no registrados.", utcNow, asTracking, cancellationToken);
    //                        return Result<RefreshResponse>.Fail(Error.Unauthorized("Credenciales no reconocidas."));
    //                    }
    //                }
    //            }
    //            else
    //            {
    //                var mismatchedUserSession = await _userSessionRepository.GetByIdAsync(currentUserSessionId, asTracking, cancellationToken);
    //                if (mismatchedUserSession is not null)
    //                {
    //                    _logger.LogWarning("Refresh: Identificador de RT malformado para sesión {SessionId}. JTI: {Jti}.", currentUserSessionId, currentJti);
    //                    await _userSessionService.RevokeUserSessionAndAccessTokenSessionAsync(mismatchedUserSession, currentJti, currentAccessTokenExpiration,
    //                        "Refresh: Refresh Token malformado.", currentUserId, currentUserSessionId, utcNow, asTracking, cancellationToken);
    //                    return Result<RefreshResponse>.Fail(Error.Validation("Formato de token erróneo."));
    //                }
    //                else
    //                {
    //                    _logger.LogWarning("Refresh: RT malformado y sesión {SessionId} inexistente. JTI: {Jti}.", currentUserSessionId, currentJti);
    //                    await _userSessionService.RevokeAccessTokenSessionAsync(currentUserSessionId, currentJti, currentAccessTokenExpiration, currentUserId,
    //                        "Refresh: RT malformado y sesión inexistente.", utcNow, asTracking, cancellationToken);
    //                    return Result<RefreshResponse>.Fail(Error.Validation("Token erróneo y sesión inválida."));
    //                }
    //            }
    //        }
    //        else
    //        {
    //            var mismatchedUserSession = await _userSessionRepository.GetByIdAsync(currentUserSessionId, asTracking, cancellationToken);
    //            if (mismatchedUserSession is not null)
    //            {
    //                _logger.LogWarning("Refresh: Intento sin enviar RT para sesión {SessionId}. JTI: {Jti}.", currentUserSessionId, currentJti);
    //                await _userSessionService.RevokeUserSessionAndAccessTokenSessionAsync(mismatchedUserSession, currentJti, currentAccessTokenExpiration,
    //                    "Refresh: Petición sin Refresh Token.", currentUserId, currentUserSessionId, utcNow, asTracking, cancellationToken);
    //                return Result<RefreshResponse>.Fail(Error.Validation("Se requiere un Refresh Token para esta operación."));
    //            }
    //            else
    //            {
    //                _logger.LogWarning("Refresh: Intento sin RT y sesión {SessionId} inexistente. JTI: {Jti}.", currentUserSessionId, currentJti);
    //                await _userSessionService.RevokeAccessTokenSessionAsync(currentUserSessionId, currentJti, currentAccessTokenExpiration, currentUserId,
    //                    "Refresh: Petición sin RT para sesión inexistente.", utcNow, asTracking, cancellationToken);
    //                return Result<RefreshResponse>.Fail(Error.Validation("Datos insuficientes para renovar la sesión."));
    //            }
    //        }
    //    }
    //}
}