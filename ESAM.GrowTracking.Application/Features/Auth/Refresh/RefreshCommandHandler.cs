using ESAM.GrowTracking.Application.Abstractions.Services;
using ESAM.GrowTracking.Application.Enums;
using ESAM.GrowTracking.Application.Extensions;
using ESAM.GrowTracking.Application.Helpers;
using ESAM.GrowTracking.Application.Results;
using ESAM.GrowTracking.Application.Settings;
using ESAM.GrowTracking.Application.ValueObjects;
using ESAM.GrowTracking.Domain.Abstractions.DataAccess;
using ESAM.GrowTracking.Domain.Abstractions.DataAccess.Repositories;
using ESAM.GrowTracking.Domain.Entities;
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
        private readonly IDateTimeService _dateTimeService;
        private readonly IUserSessionRefreshTokenRepository _userSessionRefreshTokenRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IBlacklistedTokenService _blacklistedTokenService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserSessionRepository _userSessionRepository;
        private readonly IUserSessionService _userSessionService;
        private readonly ITokenService _tokenService;
        private readonly TokenLifetimeSettings _tokenLifetimeSettings;
        private readonly ITokenSessionValidationService _tokenSessionValidationService;

        public RefreshCommandHandler(ILogger<RefreshCommandHandler> logger, IValidator<RefreshCommand> validator, IDateTimeService dateTimeService,
            IUserSessionRefreshTokenRepository userSessionRefreshTokenRepository, ICurrentUserService currentUserService, IBlacklistedTokenService blacklistedTokenService,
            IUnitOfWork unitOfWork, IUserSessionRepository userSessionRepository, IUserSessionService userSessionService, ITokenService tokenService,
            IOptions<TokenLifetimeSettings> tokenLifetimeSettingsOptions, ITokenSessionValidationService tokenSessionValidationService)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(validator);
            ArgumentNullException.ThrowIfNull(dateTimeService);
            ArgumentNullException.ThrowIfNull(userSessionRefreshTokenRepository);
            ArgumentNullException.ThrowIfNull(currentUserService);
            ArgumentNullException.ThrowIfNull(blacklistedTokenService);
            ArgumentNullException.ThrowIfNull(unitOfWork);
            ArgumentNullException.ThrowIfNull(userSessionRepository);
            ArgumentNullException.ThrowIfNull(userSessionService);
            ArgumentNullException.ThrowIfNull(tokenService);
            ArgumentNullException.ThrowIfNull(tokenLifetimeSettingsOptions);
            ArgumentNullException.ThrowIfNull(tokenSessionValidationService);
            _logger = logger;
            _validator = validator;
            _dateTimeService = dateTimeService;
            _userSessionRefreshTokenRepository = userSessionRefreshTokenRepository;
            _currentUserService = currentUserService;
            _blacklistedTokenService = blacklistedTokenService;
            _unitOfWork = unitOfWork;
            _userSessionRepository = userSessionRepository;
            _userSessionService = userSessionService;
            _tokenService = tokenService;
            _tokenLifetimeSettings = tokenLifetimeSettingsOptions.Value ?? throw new ArgumentNullException(nameof(tokenLifetimeSettingsOptions));
            _tokenSessionValidationService = tokenSessionValidationService;
        }

        public async Task<Result<RefreshResponse>> Handle(RefreshCommand request, CancellationToken cancellationToken)
        {
            var asTracking = false;
            var validation = await _validator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                _logger.LogWarning("RefreshCommand: validación fallida. Errores: {Errors}", string.Join(" | ", validation.Errors.Select(e => e.ErrorMessage)));
                return Result<RefreshResponse>.Fail(validation.ToDomainErrors());
            }
            var utcNow = _dateTimeService.UtcNow;
            RefreshTokenParser.TryParse(request.RefreshTokenRaw, out var identifier, out var refreshTokenPlain);
            var userSessionRefreshToken = identifier is not null ? await _userSessionRefreshTokenRepository.GetByIdentifierAsync(identifier, asTracking, cancellationToken) : null;
            int currentUserId;
            string? currentJti = null;
            DateTime? currentAccessTokenExpiration = null;
            UserSession? userSession;
            string revokedReasonPrefix;
            if (_currentUserService.IsAuthenticated)
            {
                currentUserId = _currentUserService.UserId!.Value;
                var currentUserSessionId = _currentUserService.UserSessionId!.Value;
                var currentUserDeviceId = _currentUserService.UserDeviceId!.Value;
                currentJti = _currentUserService.Jti;
                currentAccessTokenExpiration = _currentUserService.AccessTokenExpiration;
                revokedReasonPrefix = "Refresh (Autenticado):";
                if (userSessionRefreshToken is null)
                {
                    var currentAccessTokenType = _currentUserService.AccessTokenType!.Value;
                    if (currentAccessTokenType == AccessTokenType.Temporary)
                    {
                        var blacklistedAccessTokenTemporary = await _blacklistedTokenService.TryGenerateBlacklistedAccessTokenTemporaryAsync(currentUserId, currentJti!,
                            currentAccessTokenExpiration!.Value, utcNow, revokedReasonPrefix + " Access token temporal revocado.", currentUserId, utcNow, asTracking,
                            cancellationToken);
                        if (blacklistedAccessTokenTemporary is not null)
                        {
                            await _unitOfWork.BlacklistedAccessTokensTemporary.InsertAsync(blacklistedAccessTokenTemporary, cancellationToken);
                            await _unitOfWork.SaveChangesAsync(cancellationToken);
                        }
                        return Result<RefreshResponse>.Fail(Error.Unauthorized("No autorizado para refresh con el token actual."));
                    }
                    var targetUserSession = await _userSessionRepository.GetByIdAndUserIdAndUserDeviceIdAsync(currentUserSessionId, currentUserId, currentUserDeviceId, asTracking,
                        cancellationToken);
                    if (targetUserSession is not null)
                    {
                        _logger.LogWarning("RefreshCommand: refresh token no encontrado para sesión autenticada, revocando sesión. UserId={UserId}, UserSessionId={UserSessionId}",
                            currentUserId, currentUserSessionId);
                        await _userSessionService.RevokeUserSessionAsync(targetUserSession, currentJti, currentAccessTokenExpiration,
                            revokedReasonPrefix + " Refresh token no encontrado para esta sesión; se revoca la sesión actual por seguridad.",
                            currentUserId, utcNow, asTracking, cancellationToken);
                        return Result<RefreshResponse>.Fail(Error.Unauthorized("Refresh inválido, la sesión actual ha sido revocada por seguridad."));
                    }
                    return Result<RefreshResponse>.Fail(Error.Unauthorized("No se encontró una sesión activa para realizar refresh."));
                }
                userSession = await _userSessionRepository.GetByIdAsync(userSessionRefreshToken.UserSessionId, asTracking, cancellationToken);
                if (userSession is null || userSession.Id != currentUserSessionId || userSession.UserId != currentUserId || userSession.UserDeviceId != currentUserDeviceId)
                {
                    _logger.LogWarning("RefreshCommand: discrepancia entre sesión y refresh token en solicitud autenticada. UserId={UserId}, " +
                        "IdentificadorProvisto={Identifier}, UserSessionIdDelToken={TokenUserSessionId}, UserSessionIdActual={CurrentUserSessionId}", currentUserId, identifier!,
                        userSessionRefreshToken.UserSessionId, currentUserSessionId);
                    var targetUserSession = await _userSessionRepository.GetByIdAndUserIdAndUserDeviceIdAsync(currentUserSessionId, currentUserId, currentUserDeviceId, asTracking,
                        cancellationToken);
                    if (targetUserSession is not null)
                    {
                        _logger.LogWarning("RefreshCommand: revocando sesión actual por discrepancia. UserId={UserId}, UserSessionId={UserSessionId}", currentUserId,
                            currentUserSessionId);
                        await _userSessionService.RevokeUserSessionAsync(targetUserSession, currentJti, currentAccessTokenExpiration,
                            revokedReasonPrefix + " Refresh token o sesión no coincidentes; se revoca la sesión actual por seguridad.", currentUserId, utcNow, asTracking,
                            cancellationToken);
                        return Result<RefreshResponse>.Fail(Error.Unauthorized("Refresh inválido, se ha revocado la sesión actual por seguridad."));
                    }
                    return Result<RefreshResponse>.Fail(Error.Unauthorized("No se encontró una sesión válida para el token proporcionado."));
                }
            }
            else
            {
                if (userSessionRefreshToken is null)
                {
                    _logger.LogWarning("RefreshCommand (No Autenticado): no se proporcionó identificador de token.");
                    return Result<RefreshResponse>.Fail(Error.Unauthorized("Token de refresh no provisto o inválido."));
                }
                userSession = await _userSessionRepository.GetByIdAsync(userSessionRefreshToken.UserSessionId, asTracking, cancellationToken);
                if (userSession is null)
                {
                    _logger.LogWarning("RefreshCommand (No Autenticado): identificador de token provisto pero sesión no encontrada. Identificador={Identifier}", identifier!);
                    return Result<RefreshResponse>.Fail(Error.Unauthorized("Token inválido o sesión asociada no encontrada."));
                }
                currentUserId = userSession.UserId;
                revokedReasonPrefix = "Refresh (No Autenticado):";
            }
            var tokenSessionValidationResult = await _tokenSessionValidationService.ValidateAsync(userSession, userSessionRefreshToken!, refreshTokenPlain, 
                request.DeviceIdentifier, revokedReasonPrefix, _currentUserService.IsAuthenticated, 
                currentUserId, _currentUserService.IsAuthenticated ? _currentUserService.WorkProfileId : null, 
                _currentUserService.IsAuthenticated ? _currentUserService.RoleId : null, _currentUserService.IsAuthenticated ? _currentUserService.CampusId : null,
                _currentUserService.IsAuthenticated ? _currentUserService.TokenVersion : null, _currentUserService.IsAuthenticated ? _currentUserService.SecurityStamp : null,
                utcNow, asTracking, cancellationToken);
            if (!tokenSessionValidationResult.IsValid)
            {
                await _userSessionService.RevokeUserSessionAsync(userSession, currentJti, currentAccessTokenExpiration, tokenSessionValidationResult.RevokedReason, currentUserId, 
                    utcNow, asTracking, cancellationToken);
                return Result<RefreshResponse>.Fail(Error.Unauthorized("Refresh inválido. La sesión ha sido revocada por motivos de seguridad. Inicie sesión nuevamente."));
            }
            return await ExecuteTokenRotationAsync(userSession, userSessionRefreshToken!, tokenSessionValidationResult.ValidatedUser!, tokenSessionValidationResult.RevokedReason, 
                currentUserId, utcNow, currentJti, currentAccessTokenExpiration, tokenSessionValidationResult.WorkProfileId, tokenSessionValidationResult.RoleId, 
                tokenSessionValidationResult.CampusId, asTracking, cancellationToken);
        }

        private async Task<Result<RefreshResponse>> ExecuteTokenRotationAsync(UserSession userSession, UserSessionRefreshToken userSessionRefreshToken, User user, 
            string revokedReason, int currentUserId, DateTime utcNow, string? currentJti, DateTime? currentAccessTokenExpiration, int workProfileId, int? roleId = null, 
            int? campusId = null, bool asTracking = false, CancellationToken cancellationToken = default)
        {
            var rotatedRefreshToken = await _userSessionService.RotateUserSessionAsync(userSession, userSessionRefreshToken, currentJti, currentAccessTokenExpiration, 
                revokedReason, currentUserId, utcNow, asTracking, cancellationToken);
            var accessToken = _tokenService.GenerateSessionAccessToken(user.Id, user.SecurityStamp, user.TokenVersion, userSession.UserDeviceId, userSession.Id, utcNow,
                _tokenLifetimeSettings.SessionAccessTokenLifetimeMinutes, workProfileId, roleId, campusId);
            return Result<RefreshResponse>.Ok(new RefreshResponse(accessToken.Token, accessToken.ExpiresIn, accessToken.ExpiresAt, rotatedRefreshToken.Identifier,
                rotatedRefreshToken.Token, rotatedRefreshToken.ExpiresIn, rotatedRefreshToken.ExpiresAt));
        }
    }
}