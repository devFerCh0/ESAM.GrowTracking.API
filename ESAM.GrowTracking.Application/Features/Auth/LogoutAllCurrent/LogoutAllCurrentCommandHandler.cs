using ESAM.GrowTracking.Application.Abstractions.Services;
using ESAM.GrowTracking.Application.Results;
using ESAM.GrowTracking.Application.ValueObjects;
using ESAM.GrowTracking.Domain.Abstractions.DataAccess.Repositories;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ESAM.GrowTracking.Application.Features.Auth.LogoutAllCurrent
{
    public class LogoutAllCurrentCommandHandler : IRequestHandler<LogoutAllCurrentCommand, Result<LogoutAllCurrentResponse>>
    {
        private readonly ILogger<LogoutAllCurrentCommandHandler> _logger;
        private readonly IAccessTokenClaimsValidatorService _accessTokenClaimsValidatorService;
        private readonly IUserSessionRepository _userSessionRepository;
        private readonly IUserSessionService _userSessionService;
        private readonly IDateTimeService _dateTimeService;

        public LogoutAllCurrentCommandHandler(ILogger<LogoutAllCurrentCommandHandler> logger, IAccessTokenClaimsValidatorService accessTokenClaimsValidatorService, 
            IUserSessionRepository userSessionRepository, IUserSessionService userSessionService, IDateTimeService dateTimeService)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(accessTokenClaimsValidatorService);
            ArgumentNullException.ThrowIfNull(userSessionRepository);
            ArgumentNullException.ThrowIfNull(userSessionService);
            ArgumentNullException.ThrowIfNull(dateTimeService);
            _logger = logger;
            _accessTokenClaimsValidatorService = accessTokenClaimsValidatorService;
            _userSessionRepository = userSessionRepository;
            _userSessionService = userSessionService;
            _dateTimeService = dateTimeService;
        }

        public async Task<Result<LogoutAllCurrentResponse>> Handle(LogoutAllCurrentCommand request, CancellationToken cancellationToken)
        {
            var asTracking = false;
            var utcNow = _dateTimeService.UtcNow;
            var currentUserId = _accessTokenClaimsValidatorService.CurrentUserId;
            var currentUserSessionId = _accessTokenClaimsValidatorService.CurrentUserSessionId;
            var activeUserSessions = (await _userSessionRepository.GetActiveByUserIdAsync(currentUserId, utcNow, asTracking, cancellationToken))
                .Where(us => us.Id != currentUserSessionId).ToList();
            if (activeUserSessions.Count == 0)
            {
                _logger.LogInformation("LogoutAllCurrentCommand: No tiene sesiones de usuario activas para revocar. CurrentUserId={CurrentUserId}", currentUserId);
                return Result<LogoutAllCurrentResponse>.Fail(Error.NotFound("No tiene sesiones de usaurio activas para revocar."));
            }
            var revokedCount = await _userSessionService.RevokeCurrentUserSessionsAsync(activeUserSessions,
                $"LogoutAll: Cierre de todas las sesiones ejecutado por el usuario administrador {currentUserId}.", currentUserId, utcNow, asTracking, cancellationToken);
            _logger.LogInformation("LogoutAllCurrentCommand: {Count} sesión(es) de usaurio revocada(s) exitosamente. RevocadoPor={CurrentUserId}", revokedCount, currentUserId);
            return Result<LogoutAllCurrentResponse>.Ok(new LogoutAllCurrentResponse(revokedCount));
        }
    }
}