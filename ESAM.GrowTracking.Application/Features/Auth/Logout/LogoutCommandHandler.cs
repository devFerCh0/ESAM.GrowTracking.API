using ESAM.GrowTracking.Application.Abstractions.Services;
using ESAM.GrowTracking.Application.Enums;
using ESAM.GrowTracking.Application.Extensions;
using ESAM.GrowTracking.Application.Helpers;
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
        private readonly IDateTimeService _dateTimeService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IBlacklistedTokenService _blacklistedTokenService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserSessionRepository _userSessionRepository;
        private readonly IUserSessionService _userSessionService;

        public LogoutCommandHandler(
            ILogger<LogoutCommandHandler> logger,
            IDateTimeService dateTimeService,
            ICurrentUserService currentUserService,
            IBlacklistedTokenService blacklistedTokenService,
            IUnitOfWork unitOfWork,
            IUserSessionRepository userSessionRepository,
            IUserSessionService userSessionService)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(dateTimeService);
            ArgumentNullException.ThrowIfNull(currentUserService);
            ArgumentNullException.ThrowIfNull(blacklistedTokenService);
            ArgumentNullException.ThrowIfNull(unitOfWork);
            ArgumentNullException.ThrowIfNull(userSessionRepository);
            ArgumentNullException.ThrowIfNull(userSessionService);
            _logger = logger;
            _dateTimeService = dateTimeService;
            _currentUserService = currentUserService;
            _blacklistedTokenService = blacklistedTokenService;
            _unitOfWork = unitOfWork;
            _userSessionRepository = userSessionRepository;
            _userSessionService = userSessionService;
        }

        public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
        {
            const bool asTracking = false;
            var utcNow = _dateTimeService.UtcNow;
            var currentUserId = _currentUserService.UserId!.Value;
            var currentJti = _currentUserService.Jti;
            var currentAccessTokenExpiration = _currentUserService.AccessTokenExpiration;
            const string revokedReason = "Cerrar Sesión: revocación solicitada explícitamente por el usuario.";
            if (_currentUserService.AccessTokenType == AccessTokenType.Temporary)
            {
                var blacklistedAccessTokenTemporary = await _blacklistedTokenService.TryGenerateBlacklistedAccessTokenTemporaryAsync(
                    currentUserId, currentJti!, currentAccessTokenExpiration!.Value, utcNow, revokedReason, currentUserId, utcNow, asTracking, cancellationToken);
                if (blacklistedAccessTokenTemporary is not null)
                {
                    await _unitOfWork.BlacklistedAccessTokensTemporary.InsertAsync(blacklistedAccessTokenTemporary, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }
                return Result.Ok();
            }
            var userSession = await _userSessionRepository.GetByIdAndUserIdAndUserDeviceIdAsync(
                _currentUserService.UserSessionId!.Value, currentUserId, _currentUserService.UserDeviceId!.Value, asTracking, cancellationToken);
            if (userSession is null)
            {
                _logger.LogInformation(
                    "LogoutCommand: no se encontró sesión activa para revocar (probablemente ya revocada o expirada). UserId={UserId}, UserSessionId={UserSessionId}",
                    currentUserId, _currentUserService.UserSessionId);
                return Result.Ok();
            }

            await _userSessionService.RevokeUserSessionAsync(
                userSession, currentJti, currentAccessTokenExpiration, revokedReason, currentUserId, utcNow, asTracking, cancellationToken);
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

    //    public LogoutCommandHandler(
    //        ILogger<LogoutCommandHandler> logger,
    //        IDateTimeService dateTimeService,
    //        ICurrentUserService currentUserService,
    //        IBlacklistedTokenService blacklistedTokenService,
    //        IUnitOfWork unitOfWork,
    //        IUserSessionRepository userSessionRepository,
    //        IUserSessionService userSessionService)
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
    //        var utcNow = _dateTimeService.UtcNow;
    //        var currentUserId = _currentUserService.UserId!.Value;
    //        var currentJti = _currentUserService.Jti!;
    //        var currentExpiration = _currentUserService.AccessTokenExpiration!.Value;
    //        var currentTokenType = _currentUserService.AccessTokenType!.Value;

    //        return currentTokenType == AccessTokenType.Temporary
    //            ? await HandleTemporaryTokenAsync(currentUserId, currentJti, currentExpiration, utcNow, cancellationToken)
    //            : await HandleSessionTokenAsync(currentUserId, currentJti, currentExpiration, utcNow, cancellationToken);
    //    }

    //    private async Task<Result> HandleTemporaryTokenAsync(
    //        int userId, string jti, DateTime expiration, DateTime utcNow, CancellationToken cancellationToken)
    //    {
    //        var blacklisted = await _blacklistedTokenService.TryGenerateBlacklistedAccessTokenTemporaryAsync(
    //            userId, jti, expiration, utcNow,
    //            "Cerrar Sesión: Token temporal revocado.",
    //            userId, utcNow, false, cancellationToken);

    //        if (blacklisted is not null)
    //        {
    //            await _unitOfWork.BlacklistedAccessTokensTemporary.InsertAsync(blacklisted, cancellationToken);
    //            await _unitOfWork.SaveChangesAsync(cancellationToken);
    //        }

    //        return Result.Ok();
    //    }

    //    private async Task<Result> HandleSessionTokenAsync(
    //        int userId, string jti, DateTime expiration, DateTime utcNow, CancellationToken cancellationToken)
    //    {
    //        var currentUserSessionId = _currentUserService.UserSessionId!.Value;
    //        var currentUserDeviceId = _currentUserService.UserDeviceId!.Value;

    //        var userSession = await _userSessionRepository.GetByIdAndUserIdAndUserDeviceIdAsync(
    //            currentUserSessionId, userId, currentUserDeviceId, false, cancellationToken);

    //        if (userSession is null)
    //        {
    //            _logger.LogWarning(
    //                "LogoutCommand: sesión no encontrada o ya inactiva. UserId={UserId}, UserSessionId={UserSessionId}",
    //                userId, currentUserSessionId);

    //            return Result.Ok();
    //        }

    //        await _userSessionService.RevokeUserSessionAsync(
    //            userSession, jti, expiration,
    //            "Cerrar Sesión: Solicitud del usuario.",
    //            userId, utcNow, false, cancellationToken);

    //        return Result.Ok();
    //    }
    //}

    //public sealed class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result>
    //{
    //    private readonly ILogger<LogoutCommandHandler> _logger;
    //    private readonly IDateTimeService _dateTimeService;
    //    private readonly ICurrentUserService _currentUserService;
    //    private readonly IBlacklistedTokenService _blacklistedTokenService;
    //    private readonly IUnitOfWork _unitOfWork;
    //    private readonly IUserSessionRepository _userSessionRepository;
    //    private readonly IUserSessionService _userSessionService;

    //    public LogoutCommandHandler(
    //        ILogger<LogoutCommandHandler> logger,
    //        IDateTimeService dateTimeService,
    //        ICurrentUserService currentUserService,
    //        IBlacklistedTokenService blacklistedTokenService,
    //        IUnitOfWork unitOfWork,
    //        IUserSessionRepository userSessionRepository,
    //        IUserSessionService userSessionService)
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
    //        // [Authorize] garantiza IsAuthenticated = true y firma del AT verificada por middleware.
    //        // El handler nunca llega con un AT inválido o ausente.
    //        const bool asTracking = false;
    //        const string revokedReasonPrefix = "Cerrar Sesión:";

    //        var utcNow = _dateTimeService.UtcNow;
    //        var currentUserId = _currentUserService.UserId!.Value;
    //        var currentJti = _currentUserService.Jti;
    //        var currentAccessTokenExpiration = _currentUserService.AccessTokenExpiration;
    //        var currentAccessTokenType = _currentUserService.AccessTokenType!.Value;

    //        // AT Temporary: no existe sesión en DB ni RT asociado.
    //        // Se invalida el JTI para cerrar la ventana residual de uso del token.
    //        if (currentAccessTokenType == AccessTokenType.Temporary)
    //        {
    //            var blacklisted = await _blacklistedTokenService.TryGenerateBlacklistedAccessTokenTemporaryAsync(
    //                currentUserId, currentJti!, currentAccessTokenExpiration!.Value, utcNow,
    //                revokedReasonPrefix + " Access token temporal revocado.",
    //                currentUserId, utcNow, asTracking, cancellationToken);

    //            if (blacklisted is not null)
    //            {
    //                await _unitOfWork.BlacklistedAccessTokensTemporary.InsertAsync(blacklisted, cancellationToken);
    //                await _unitOfWork.SaveChangesAsync(cancellationToken);
    //            }

    //            return Result.Ok();
    //        }

    //        // AT Session: la sesión se identifica directamente desde los claims del AT firmado.
    //        // No se requiere ningún dato del cliente — el AT es la única credencial de este endpoint.
    //        var currentUserSessionId = _currentUserService.UserSessionId;
    //        var currentUserDeviceId = _currentUserService.UserDeviceId;

    //        if (!currentUserSessionId.HasValue || !currentUserDeviceId.HasValue)
    //        {
    //            // AT de tipo Session emitido sin userSessionId o userDeviceId: error de generación.
    //            // Se registra como advertencia. El acceso ya está denegado por la ausencia de contexto de sesión.
    //            _logger.LogWarning(
    //                "LogoutCommand: AT de tipo Session sin claims de sesión o dispositivo. UserId={UserId}", currentUserId);
    //            return Result.Ok();
    //        }

    //        // Lookup triple para garantizar que la sesión pertenece a este usuario y dispositivo.
    //        // Cubre escenario donde un AT robado de otra sesión intenta revocar una sesión ajena.
    //        var userSession = await _userSessionRepository.GetByIdAndUserIdAndUserDeviceIdAsync(
    //            currentUserSessionId.Value, currentUserId, currentUserDeviceId.Value, asTracking, cancellationToken);

    //        if (userSession is null)
    //        {
    //            // Sesión ya revocada, expirada o inexistente. Logout es idempotente por diseño.
    //            return Result.Ok();
    //        }

    //        await _userSessionService.RevokeUserSessionAsync(
    //            userSession, currentJti, currentAccessTokenExpiration,
    //            revokedReasonPrefix + " Logout exitoso.",
    //            currentUserId, utcNow, asTracking, cancellationToken);

    //        return Result.Ok();
    //    }
    //}

    //////public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result>
    //////{
    //////    private readonly ILogger<LogoutCommandHandler> _logger;
    //////    private readonly IValidator<LogoutCommand> _validator;
    //////    private readonly IDateTimeService _dateTimeService;
    //////    private readonly IUserSessionRefreshTokenRepository _userSessionRefreshTokenRepository;
    //////    private readonly ICurrentUserService _currentUserService;
    //////    private readonly IBlacklistedTokenService _blacklistedTokenService;
    //////    private readonly IUnitOfWork _unitOfWork;
    //////    private readonly IUserSessionRepository _userSessionRepository;
    //////    private readonly IUserSessionService _userSessionService;
    //////    private readonly ITokenSessionValidationService _tokenSessionValidationService;

    //////    public LogoutCommandHandler(ILogger<LogoutCommandHandler> logger, IValidator<LogoutCommand> validator, IDateTimeService dateTimeService,
    //////        IUserSessionRefreshTokenRepository userSessionRefreshTokenRepository, ICurrentUserService currentUserService, IBlacklistedTokenService blacklistedTokenService,
    //////        IUnitOfWork unitOfWork, IUserSessionRepository userSessionRepository, IUserSessionService userSessionService,
    //////        ITokenSessionValidationService tokenSessionValidationService)
    //////    {
    //////        ArgumentNullException.ThrowIfNull(logger);
    //////        ArgumentNullException.ThrowIfNull(validator);
    //////        ArgumentNullException.ThrowIfNull(dateTimeService);
    //////        ArgumentNullException.ThrowIfNull(userSessionRefreshTokenRepository);
    //////        ArgumentNullException.ThrowIfNull(currentUserService);
    //////        ArgumentNullException.ThrowIfNull(blacklistedTokenService);
    //////        ArgumentNullException.ThrowIfNull(unitOfWork);
    //////        ArgumentNullException.ThrowIfNull(userSessionRepository);
    //////        ArgumentNullException.ThrowIfNull(userSessionService);
    //////        ArgumentNullException.ThrowIfNull(tokenSessionValidationService);
    //////        _logger = logger;
    //////        _validator = validator;
    //////        _dateTimeService = dateTimeService;
    //////        _userSessionRefreshTokenRepository = userSessionRefreshTokenRepository;
    //////        _currentUserService = currentUserService;
    //////        _blacklistedTokenService = blacklistedTokenService;
    //////        _unitOfWork = unitOfWork;
    //////        _userSessionRepository = userSessionRepository;
    //////        _userSessionService = userSessionService;
    //////        _tokenSessionValidationService = tokenSessionValidationService;
    //////    }

    //////    public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
    //////    {
    //////        var asTracking = false;
    //////        var validation = await _validator.ValidateAsync(request, cancellationToken);
    //////        if (!validation.IsValid)
    //////        {
    //////            _logger.LogWarning("LogoutCommand: validación fallida. Errores: {Errors}", string.Join(" | ", validation.Errors.Select(e => e.ErrorMessage)));
    //////            return Result.Fail(validation.ToDomainErrors());
    //////        }
    //////        var utcNow = _dateTimeService.UtcNow;
    //////        RefreshTokenParser.TryParse(request.RefreshTokenRaw, out var identifier, out var refreshTokenPlain);
    //////        var userSessionRefreshToken = identifier is not null ? await _userSessionRefreshTokenRepository.GetByIdentifierAsync(identifier, asTracking, cancellationToken) : null;
    //////        int currentUserId;
    //////        string? currentJti = null;
    //////        DateTime? currentAccessTokenExpiration = null;
    //////        UserSession? userSession;
    //////        string revokedReasonPrefix;
    //////        if (_currentUserService.IsAuthenticated)
    //////        {
    //////            currentUserId = _currentUserService.UserId!.Value;
    //////            var currentUserSessionId = _currentUserService.UserSessionId!.Value;
    //////            var currentUserDeviceId = _currentUserService.UserDeviceId!.Value;
    //////            currentJti = _currentUserService.Jti;
    //////            currentAccessTokenExpiration = _currentUserService.AccessTokenExpiration;
    //////            revokedReasonPrefix = "Cerrar Sesión (Autenticado):";
    //////            if (userSessionRefreshToken is null)
    //////            {
    //////                var currentAccessTokenType = _currentUserService.AccessTokenType!.Value;
    //////                if (currentAccessTokenType == AccessTokenType.Temporary)
    //////                {
    //////                    var blacklistedAccessTokenTemporary = await _blacklistedTokenService.TryGenerateBlacklistedAccessTokenTemporaryAsync(currentUserId, currentJti!,
    //////                        currentAccessTokenExpiration!.Value, utcNow, revokedReasonPrefix + " Access token temporal revocado.", currentUserId, utcNow, asTracking,
    //////                        cancellationToken);
    //////                    if (blacklistedAccessTokenTemporary is not null)
    //////                    {
    //////                        await _unitOfWork.BlacklistedAccessTokensTemporary.InsertAsync(blacklistedAccessTokenTemporary, cancellationToken);
    //////                        await _unitOfWork.SaveChangesAsync(cancellationToken);
    //////                    }
    //////                    return Result.Ok();
    //////                }
    //////                var targetUserSession = await _userSessionRepository.GetByIdAndUserIdAndUserDeviceIdAsync(currentUserSessionId, currentUserId, currentUserDeviceId, asTracking,
    //////                    cancellationToken);
    //////                if (targetUserSession is not null)
    //////                {
    //////                    _logger.LogWarning("LogoutCommand: refresh token no encontrado para sesión autenticada, revocando sesión por seguridad. UserId={UserId}, " +
    //////                        "UserSessionId={UserSessionId}", currentUserId, currentUserSessionId);
    //////                    await _userSessionService.RevokeUserSessionAsync(targetUserSession, currentJti, currentAccessTokenExpiration,
    //////                        revokedReasonPrefix + " Refresh token no encontrado para esta sesión; se revoca la sesión actual por seguridad.",
    //////                        currentUserId, utcNow, asTracking, cancellationToken);
    //////                }
    //////                return Result.Ok();
    //////            }
    //////            userSession = await _userSessionRepository.GetByIdAsync(userSessionRefreshToken.UserSessionId, asTracking, cancellationToken);
    //////            if (userSession is null || userSession.Id != currentUserSessionId || userSession.UserId != currentUserId || userSession.UserDeviceId != currentUserDeviceId)
    //////            {
    //////                _logger.LogWarning("LogoutCommand: discrepancia entre sesión y refresh token. UserId={UserId}, IdentificadorProvisto={Identifier}, " +
    //////                    "UserSessionIdDelToken={TokenUserSessionId}, UserSessionIdActual={CurrentUserSessionId}", currentUserId, identifier!, userSessionRefreshToken.UserSessionId,
    //////                    currentUserSessionId);
    //////                var targetUserSession = await _userSessionRepository.GetByIdAndUserIdAndUserDeviceIdAsync(currentUserSessionId, currentUserId, currentUserDeviceId, asTracking,
    //////                    cancellationToken);
    //////                if (targetUserSession is not null)
    //////                {
    //////                    _logger.LogWarning("LogoutCommand: revocando sesión actual por discrepancia. UserId={UserId}, UserSessionId={UserSessionId}", currentUserId,
    //////                        currentUserSessionId);
    //////                    await _userSessionService.RevokeUserSessionAsync(targetUserSession, currentJti, currentAccessTokenExpiration,
    //////                        revokedReasonPrefix + " Refresh token o sesión no coincidentes; se revoca la sesión actual por seguridad.", currentUserId, utcNow, asTracking,
    //////                        cancellationToken);
    //////                }
    //////                return Result.Ok();
    //////            }
    //////        }
    //////        else
    //////        {
    //////            if (userSessionRefreshToken is null)
    //////                return Result.Ok();
    //////            userSession = await _userSessionRepository.GetByIdAsync(userSessionRefreshToken.UserSessionId, asTracking, cancellationToken);
    //////            if (userSession is null)
    //////                return Result.Ok();
    //////            currentUserId = userSession.UserId;
    //////            revokedReasonPrefix = "Cerrar Sesión (No Autenticado):";
    //////        }
    //////        var tokenSessionValidationResult = await _tokenSessionValidationService.ValidateAsync(userSession, userSessionRefreshToken!, refreshTokenPlain,
    //////            request.DeviceIdentifier, revokedReasonPrefix, _currentUserService.IsAuthenticated, currentUserId,
    //////            _currentUserService.IsAuthenticated ? _currentUserService.WorkProfileId : null,
    //////            _currentUserService.IsAuthenticated ? _currentUserService.RoleId : null, _currentUserService.IsAuthenticated ? _currentUserService.CampusId : null,
    //////            _currentUserService.IsAuthenticated ? _currentUserService.TokenVersion : null, _currentUserService.IsAuthenticated ? _currentUserService.SecurityStamp : null,
    //////            utcNow, asTracking, cancellationToken);
    //////        await _userSessionService.RevokeUserSessionAsync(userSession, currentJti, currentAccessTokenExpiration, tokenSessionValidationResult.RevokedReason, currentUserId,
    //////            utcNow, asTracking, cancellationToken);
    //////        return Result.Ok();
    //////    }
    //////}
}