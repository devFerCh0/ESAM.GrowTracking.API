using ESAM.GrowTracking.Application.Abstractions.Services;
using ESAM.GrowTracking.Application.Enums;
using ESAM.GrowTracking.Application.Results;
using ESAM.GrowTracking.Domain.Abstractions.DataAccess;
using ESAM.GrowTracking.Domain.Abstractions.DataAccess.Repositories;
using ESAM.GrowTracking.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ESAM.GrowTracking.Application.Features.Auth.Logout
{
    public sealed class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result>
    {
        private readonly ILogger<LogoutCommandHandler> _logger;
        private readonly IValidator<LogoutCommand> _validator;
        private readonly IDateTimeService _dateTimeService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IBlacklistedTokenService _blacklistedTokenService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserSessionRepository _userSessionRepository;
        private readonly IUserSessionRefreshTokenRepository _userSessionRefreshTokenRepository;
        private readonly IUserSessionService _userSessionService;

        public LogoutCommandHandler(ILogger<LogoutCommandHandler> logger, IValidator<LogoutCommand> validator, IDateTimeService dateTimeService,
            ICurrentUserService currentUserService, IBlacklistedTokenService blacklistedTokenService, IUnitOfWork unitOfWork, IUserSessionRepository userSessionRepository,
            IUserSessionRefreshTokenRepository userSessionRefreshTokenRepository, IUserSessionService userSessionService)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(validator);
            ArgumentNullException.ThrowIfNull(dateTimeService);
            ArgumentNullException.ThrowIfNull(currentUserService);
            ArgumentNullException.ThrowIfNull(blacklistedTokenService);
            ArgumentNullException.ThrowIfNull(unitOfWork);
            ArgumentNullException.ThrowIfNull(userSessionRepository);
            ArgumentNullException.ThrowIfNull(userSessionRefreshTokenRepository);
            ArgumentNullException.ThrowIfNull(userSessionService);
            _logger = logger;
            _validator = validator;
            _dateTimeService = dateTimeService;
            _currentUserService = currentUserService;
            _blacklistedTokenService = blacklistedTokenService;
            _unitOfWork = unitOfWork;
            _userSessionRepository = userSessionRepository;
            _userSessionRefreshTokenRepository = userSessionRefreshTokenRepository;
            _userSessionService = userSessionService;
        }

        public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
        {
            var validation = await _validator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                _logger.LogWarning("LogoutCommand: Validación fallida. Errores: {Errors}", string.Join(" | ", validation.Errors.Select(e => e.ErrorMessage)));
                return Result.Fail(validation.ToDomainErrors());
            }
            const bool asTracking = false;
            const string revokedReasonPrefix = "Cerrar Sesión:";
            var utcNow = _dateTimeService.UtcNow;
            var currentUserId = _currentUserService.UserId!.Value;
            var currentJti = _currentUserService.Jti;
            var currentAtExpiration = _currentUserService.AccessTokenExpiration;
            var currentAtType = _currentUserService.AccessTokenType!.Value;
            if (currentAtType == AccessTokenType.Temporary)
            {
                var blacklisted = await _blacklistedTokenService.TryGenerateBlacklistedAccessTokenTemporaryAsync(currentUserId, currentJti!, currentAtExpiration!.Value, utcNow,
                    $"{revokedReasonPrefix} Access token temporal revocado.", currentUserId, utcNow, asTracking, cancellationToken);
                if (blacklisted is not null)
                {
                    await _unitOfWork.BlacklistedAccessTokensTemporary.InsertAsync(blacklisted, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }
                return Result.Ok();
            }
            var currentSessionId = _currentUserService.UserSessionId;
            var currentDeviceId = _currentUserService.UserDeviceId;
            if (!currentSessionId.HasValue || !currentDeviceId.HasValue)
            {
                _logger.LogWarning("LogoutCommand: Access Token tipo Session carece de UserSessionId o UserDeviceId. UserId={UserId}", currentUserId);
                return Result.Ok();
            }
            RefreshTokenParser.TryParse(request.RefreshTokenRaw, out var identifier, out _);
            UserSession? sessionToRevoke = null;
            string finalRevokedReason = string.Empty;
            if (identifier is not null)
            {
                var userSessionRefreshToken = await _userSessionRefreshTokenRepository.GetByIdentifierAsync(identifier, asTracking, cancellationToken);
                if (userSessionRefreshToken is null)
                {
                    _logger.LogWarning("LogoutCommand: RT provisto no encontrado en BD. UserId={UserId}, Identifier={Identifier}", currentUserId, identifier);
                    finalRevokedReason = $"{revokedReasonPrefix} Logout ejecutado por claims del AT (RT no encontrado).";
                }
                else if (userSessionRefreshToken.UserSessionId != currentSessionId.Value)
                {
                    _logger.LogWarning("LogoutCommand: Discrepancia crítica binding AT-RT. UserId={UserId}, AT_Session={ATSession}, RT_Session={RTSession}", currentUserId,
                        currentSessionId.Value, userSessionRefreshToken.UserSessionId);
                    finalRevokedReason = $"{revokedReasonPrefix} Binding AT-RT no coincidente; sesión revocada por prevención de seguridad.";
                }
                else
                {
                    sessionToRevoke = await _userSessionRepository.GetByIdAsync(currentSessionId.Value, asTracking, cancellationToken);
                    if (sessionToRevoke is null || sessionToRevoke.UserId != currentUserId || sessionToRevoke.UserDeviceId != currentDeviceId.Value)
                    {
                        _logger.LogWarning("LogoutCommand: Discrepancia en propiedad de sesión. UserId={UserId}, SessionId={SessionId}", currentUserId, currentSessionId.Value);
                        sessionToRevoke = null;
                    }
                    else
                        finalRevokedReason = $"{revokedReasonPrefix} Logout exitoso con binding AT-RT verificado.";
                }
            }
            else
                finalRevokedReason = $"{revokedReasonPrefix} Logout ejecutado por claims del AT (RT no provisto).";
            if (sessionToRevoke is null)
            {
                sessionToRevoke = await _userSessionRepository.GetByIdAndUserIdAndUserDeviceIdAsync(currentSessionId.Value, currentUserId, currentDeviceId.Value, asTracking,
                    cancellationToken);
                if (string.IsNullOrEmpty(finalRevokedReason))
                    finalRevokedReason = $"{revokedReasonPrefix} Logout ejecutado por claims del AT.";
            }
            if (sessionToRevoke is not null)
                await _userSessionService.RevokeUserSessionAsync(sessionToRevoke, currentJti, currentAtExpiration, finalRevokedReason, currentUserId, utcNow, asTracking,
                    cancellationToken);
            return Result.Ok();
        }
    }

    //public sealed class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result>
    //{
    //    private readonly ILogger<LogoutCommandHandler> _logger;
    //    private readonly IDateTimeService _dateTimeService;
    //    private readonly ICurrentUserService _currentUserService;
    //    private readonly IBlacklistedTokenService _blacklistedTokenService;
    //    private readonly IUnitOfWork _unitOfWork;
    //    private readonly IUserSessionRepository _userSessionRepository;
    //    private readonly IUserSessionService _userSessionService;

    //    public LogoutCommandHandler(ILogger<LogoutCommandHandler> logger, IDateTimeService dateTimeService, ICurrentUserService currentUserService, 
    //        IBlacklistedTokenService blacklistedTokenService, IUnitOfWork unitOfWork, IUserSessionRepository userSessionRepository, IUserSessionService userSessionService)
    //    {
    //        ArgumentNullException.ThrowIfNull(logger);
    //        ArgumentNullException.ThrowIfNull(dateTimeService);
    //        ArgumentNullException.ThrowIfNull(currentUserService);
    //        ArgumentNullException.ThrowIfNull(blacklistedTokenService);
    //        ArgumentNullException.ThrowIfNull(unitOfWork);
    //        ArgumentNullException.ThrowIfNull(userSessionRepository);
    //        ArgumentNullException.ThrowIfNull(userSessionService);
    //        _logger = logger;
    //        _dateTimeService = dateTimeService;
    //        _currentUserService = currentUserService;
    //        _blacklistedTokenService = blacklistedTokenService;
    //        _unitOfWork = unitOfWork;
    //        _userSessionRepository = userSessionRepository;
    //        _userSessionService = userSessionService;
    //    }

    //    public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
    //    {
    //        const bool asTracking = false;
    //        const string revokedReasonPrefix = "Cerrar Sesión:";
    //        var utcNow = _dateTimeService.UtcNow;
    //        var currentUserId = _currentUserService.UserId!.Value;
    //        var currentJti = _currentUserService.Jti;
    //        var currentAccessTokenExpiration = _currentUserService.AccessTokenExpiration;
    //        var currentAccessTokenType = _currentUserService.AccessTokenType!.Value;
    //        if (currentAccessTokenType == AccessTokenType.Temporary)
    //        {
    //            var blacklisted = await _blacklistedTokenService.TryGenerateBlacklistedAccessTokenTemporaryAsync(currentUserId, currentJti!, currentAccessTokenExpiration!.Value, 
    //                utcNow, revokedReasonPrefix + " Access token temporal revocado.", currentUserId, utcNow, asTracking, cancellationToken);
    //            if (blacklisted is not null)
    //            {
    //                await _unitOfWork.BlacklistedAccessTokensTemporary.InsertAsync(blacklisted, cancellationToken);
    //                await _unitOfWork.SaveChangesAsync(cancellationToken);
    //            }
    //            return Result.Ok();
    //        }
    //        var currentUserSessionId = _currentUserService.UserSessionId;
    //        var currentUserDeviceId = _currentUserService.UserDeviceId;
    //        if (!currentUserSessionId.HasValue || !currentUserDeviceId.HasValue)
    //        {
    //            _logger.LogWarning("LogoutCommand: AT de tipo Session sin claims de sesión o dispositivo. UserId={UserId}", currentUserId);
    //            return Result.Ok();
    //        }
    //        var userSession = await _userSessionRepository.GetByIdAndUserIdAndUserDeviceIdAsync(currentUserSessionId.Value, currentUserId, currentUserDeviceId.Value, asTracking, 
    //            cancellationToken);
    //        if (userSession is not null)
    //            await _userSessionService.RevokeUserSessionAsync(userSession, currentJti, currentAccessTokenExpiration, revokedReasonPrefix + " Logout exitoso.", currentUserId, 
    //                utcNow, asTracking, cancellationToken);
    //        return Result.Ok();
    //    }
    //}

    ////public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result>
    ////{
    ////    private readonly ILogger<LogoutCommandHandler> _logger;
    ////    private readonly IValidator<LogoutCommand> _validator;
    ////    private readonly IDateTimeService _dateTimeService;
    ////    private readonly IUserSessionRefreshTokenRepository _userSessionRefreshTokenRepository;
    ////    private readonly ICurrentUserService _currentUserService;
    ////    private readonly IBlacklistedTokenService _blacklistedTokenService;
    ////    private readonly IUnitOfWork _unitOfWork;
    ////    private readonly IUserSessionRepository _userSessionRepository;
    ////    private readonly IUserSessionService _userSessionService;
    ////    private readonly ITokenSessionValidationService _tokenSessionValidationService;

    ////    public LogoutCommandHandler(ILogger<LogoutCommandHandler> logger, IValidator<LogoutCommand> validator, IDateTimeService dateTimeService,
    ////        IUserSessionRefreshTokenRepository userSessionRefreshTokenRepository, ICurrentUserService currentUserService, IBlacklistedTokenService blacklistedTokenService,
    ////        IUnitOfWork unitOfWork, IUserSessionRepository userSessionRepository, IUserSessionService userSessionService,
    ////        ITokenSessionValidationService tokenSessionValidationService)
    ////    {
    ////        ArgumentNullException.ThrowIfNull(logger);
    ////        ArgumentNullException.ThrowIfNull(validator);
    ////        ArgumentNullException.ThrowIfNull(dateTimeService);
    ////        ArgumentNullException.ThrowIfNull(userSessionRefreshTokenRepository);
    ////        ArgumentNullException.ThrowIfNull(currentUserService);
    ////        ArgumentNullException.ThrowIfNull(blacklistedTokenService);
    ////        ArgumentNullException.ThrowIfNull(unitOfWork);
    ////        ArgumentNullException.ThrowIfNull(userSessionRepository);
    ////        ArgumentNullException.ThrowIfNull(userSessionService);
    ////        ArgumentNullException.ThrowIfNull(tokenSessionValidationService);
    ////        _logger = logger;
    ////        _validator = validator;
    ////        _dateTimeService = dateTimeService;
    ////        _userSessionRefreshTokenRepository = userSessionRefreshTokenRepository;
    ////        _currentUserService = currentUserService;
    ////        _blacklistedTokenService = blacklistedTokenService;
    ////        _unitOfWork = unitOfWork;
    ////        _userSessionRepository = userSessionRepository;
    ////        _userSessionService = userSessionService;
    ////        _tokenSessionValidationService = tokenSessionValidationService;
    ////    }

    ////    public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
    ////    {
    ////        var asTracking = false;
    ////        var validation = await _validator.ValidateAsync(request, cancellationToken);
    ////        if (!validation.IsValid)
    ////        {
    ////            _logger.LogWarning("LogoutCommand: validación fallida. Errores: {Errors}", string.Join(" | ", validation.Errors.Select(e => e.ErrorMessage)));
    ////            return Result.Fail(validation.ToDomainErrors());
    ////        }
    ////        var utcNow = _dateTimeService.UtcNow;
    ////        RefreshTokenParser.TryParse(request.RefreshTokenRaw, out var identifier, out var refreshTokenPlain);
    ////        var userSessionRefreshToken = identifier is not null ? await _userSessionRefreshTokenRepository.GetByIdentifierAsync(identifier, asTracking, cancellationToken) : null;
    ////        int currentUserId;
    ////        string? currentJti = null;
    ////        DateTime? currentAccessTokenExpiration = null;
    ////        UserSession? userSession;
    ////        string revokedReasonPrefix;
    ////        if (_currentUserService.IsAuthenticated)
    ////        {
    ////            currentUserId = _currentUserService.UserId!.Value;
    ////            var currentUserSessionId = _currentUserService.UserSessionId!.Value;
    ////            var currentUserDeviceId = _currentUserService.UserDeviceId!.Value;
    ////            currentJti = _currentUserService.Jti;
    ////            currentAccessTokenExpiration = _currentUserService.AccessTokenExpiration;
    ////            revokedReasonPrefix = "Cerrar Sesión (Autenticado):";
    ////            if (userSessionRefreshToken is null)
    ////            {
    ////                var currentAccessTokenType = _currentUserService.AccessTokenType!.Value;
    ////                if (currentAccessTokenType == AccessTokenType.Temporary)
    ////                {
    ////                    var blacklistedAccessTokenTemporary = await _blacklistedTokenService.TryGenerateBlacklistedAccessTokenTemporaryAsync(currentUserId, currentJti!,
    ////                        currentAccessTokenExpiration!.Value, utcNow, revokedReasonPrefix + " Access token temporal revocado.", currentUserId, utcNow, asTracking,
    ////                        cancellationToken);
    ////                    if (blacklistedAccessTokenTemporary is not null)
    ////                    {
    ////                        await _unitOfWork.BlacklistedAccessTokensTemporary.InsertAsync(blacklistedAccessTokenTemporary, cancellationToken);
    ////                        await _unitOfWork.SaveChangesAsync(cancellationToken);
    ////                    }
    ////                    return Result.Ok();
    ////                }
    ////                var targetUserSession = await _userSessionRepository.GetByIdAndUserIdAndUserDeviceIdAsync(currentUserSessionId, currentUserId, currentUserDeviceId, asTracking,
    ////                    cancellationToken);
    ////                if (targetUserSession is not null)
    ////                {
    ////                    _logger.LogWarning("LogoutCommand: refresh token no encontrado para sesión autenticada, revocando sesión por seguridad. UserId={UserId}, " +
    ////                        "UserSessionId={UserSessionId}", currentUserId, currentUserSessionId);
    ////                    await _userSessionService.RevokeUserSessionAsync(targetUserSession, currentJti, currentAccessTokenExpiration,
    ////                        revokedReasonPrefix + " Refresh token no encontrado para esta sesión; se revoca la sesión actual por seguridad.",
    ////                        currentUserId, utcNow, asTracking, cancellationToken);
    ////                }
    ////                return Result.Ok();
    ////            }
    ////            userSession = await _userSessionRepository.GetByIdAsync(userSessionRefreshToken.UserSessionId, asTracking, cancellationToken);
    ////            if (userSession is null || userSession.Id != currentUserSessionId || userSession.UserId != currentUserId || userSession.UserDeviceId != currentUserDeviceId)
    ////            {
    ////                _logger.LogWarning("LogoutCommand: discrepancia entre sesión y refresh token. UserId={UserId}, IdentificadorProvisto={Identifier}, " +
    ////                    "UserSessionIdDelToken={TokenUserSessionId}, UserSessionIdActual={CurrentUserSessionId}", currentUserId, identifier!, userSessionRefreshToken.UserSessionId,
    ////                    currentUserSessionId);
    ////                var targetUserSession = await _userSessionRepository.GetByIdAndUserIdAndUserDeviceIdAsync(currentUserSessionId, currentUserId, currentUserDeviceId, asTracking,
    ////                    cancellationToken);
    ////                if (targetUserSession is not null)
    ////                {
    ////                    _logger.LogWarning("LogoutCommand: revocando sesión actual por discrepancia. UserId={UserId}, UserSessionId={UserSessionId}", currentUserId,
    ////                        currentUserSessionId);
    ////                    await _userSessionService.RevokeUserSessionAsync(targetUserSession, currentJti, currentAccessTokenExpiration,
    ////                        revokedReasonPrefix + " Refresh token o sesión no coincidentes; se revoca la sesión actual por seguridad.", currentUserId, utcNow, asTracking,
    ////                        cancellationToken);
    ////                }
    ////                return Result.Ok();
    ////            }
    ////        }
    ////        else
    ////        {
    ////            if (userSessionRefreshToken is null)
    ////                return Result.Ok();
    ////            userSession = await _userSessionRepository.GetByIdAsync(userSessionRefreshToken.UserSessionId, asTracking, cancellationToken);
    ////            if (userSession is null)
    ////                return Result.Ok();
    ////            currentUserId = userSession.UserId;
    ////            revokedReasonPrefix = "Cerrar Sesión (No Autenticado):";
    ////        }
    ////        var tokenSessionValidationResult = await _tokenSessionValidationService.ValidateAsync(userSession, userSessionRefreshToken!, refreshTokenPlain,
    ////            request.DeviceIdentifier, revokedReasonPrefix, _currentUserService.IsAuthenticated, currentUserId,
    ////            _currentUserService.IsAuthenticated ? _currentUserService.WorkProfileId : null,
    ////            _currentUserService.IsAuthenticated ? _currentUserService.RoleId : null, _currentUserService.IsAuthenticated ? _currentUserService.CampusId : null,
    ////            _currentUserService.IsAuthenticated ? _currentUserService.TokenVersion : null, _currentUserService.IsAuthenticated ? _currentUserService.SecurityStamp : null,
    ////            utcNow, asTracking, cancellationToken);
    ////        await _userSessionService.RevokeUserSessionAsync(userSession, currentJti, currentAccessTokenExpiration, tokenSessionValidationResult.RevokedReason, currentUserId,
    ////            utcNow, asTracking, cancellationToken);
    ////        return Result.Ok();
    ////    }
    ////}
}