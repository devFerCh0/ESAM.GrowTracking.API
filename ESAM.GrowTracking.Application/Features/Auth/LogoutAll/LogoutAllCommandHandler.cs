using ESAM.GrowTracking.Application.Abstractions.Services;
using ESAM.GrowTracking.Application.Extensions;
using ESAM.GrowTracking.Application.Results;
using ESAM.GrowTracking.Application.ValueObjects;
using ESAM.GrowTracking.Domain.Abstractions.DataAccess.Repositories;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ESAM.GrowTracking.Application.Features.Auth.LogoutAll
{
    public class LogoutAllCommandHandler : IRequestHandler<LogoutAllCommand, Result<LogoutAllResponse>>
    {
        private readonly ILogger<LogoutAllCommandHandler> _logger;
        private readonly IValidator<LogoutAllCommand> _validator;
        private readonly IAccessTokenClaimsValidatorService _accessTokenClaimsValidatorService;
        private readonly IUserRepository _userRepository;
        private readonly IUserSessionRepository _userSessionRepository;
        private readonly IUserSessionService _userSessionService;
        private readonly IDateTimeService _dateTimeService;

        public LogoutAllCommandHandler(ILogger<LogoutAllCommandHandler> logger, IValidator<LogoutAllCommand> validator,
            IAccessTokenClaimsValidatorService accessTokenClaimsValidatorService, IUserRepository userRepository, IUserSessionRepository userSessionRepository, 
            IUserSessionService userSessionService,
            IDateTimeService dateTimeService)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(validator);
            ArgumentNullException.ThrowIfNull(accessTokenClaimsValidatorService);
            ArgumentNullException.ThrowIfNull(userRepository);
            ArgumentNullException.ThrowIfNull(userSessionRepository);
            ArgumentNullException.ThrowIfNull(userSessionService);
            ArgumentNullException.ThrowIfNull(dateTimeService);
            _logger = logger;
            _validator = validator;
            _accessTokenClaimsValidatorService = accessTokenClaimsValidatorService;
            _userRepository = userRepository;
            _userSessionRepository = userSessionRepository;
            _userSessionService = userSessionService;
            _dateTimeService = dateTimeService;
        }

        public async Task<Result<LogoutAllResponse>> Handle(LogoutAllCommand request, CancellationToken cancellationToken)
        {
            var asTracking = false;
            var validation = await _validator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                _logger.LogWarning("LogoutAllCommand: validación fallida. Errores: {Errors}", string.Join(" | ", validation.Errors.Select(e => e.ErrorMessage)));
                return Result<LogoutAllResponse>.Fail(validation.ToCommandErrors());
            }
            if (!await _userRepository.ExistsAsync(request.UserId, asTracking, cancellationToken))
            {
                _logger.LogWarning("LogoutAllCommand: usuario seleccionado no encontrado. UserId={UserId}", request.UserId);
                return Result<LogoutAllResponse>.Fail(Error.NotFound("El usuario seleccionado no fue encontrado."));
            }
            var utcNow = _dateTimeService.UtcNow;
            var activeUserSessions = await _userSessionRepository.GetActiveByUserIdAsync(request.UserId, utcNow, asTracking, cancellationToken);
            var currentUserId = _accessTokenClaimsValidatorService.CurrentUserId;
            if (activeUserSessions.Count == 0)
            {
                _logger.LogInformation("LogoutAllCommand: el usuario no tiene sesiones de usuario activas para revocar. UserId={UserId}, CurrentUserId={CurrentUserId}",
                    request.UserId, currentUserId);
                return Result<LogoutAllResponse>.Fail(Error.NotFound("El usuario no tiene sesiones de usaurio activas para revocar."));
            }
            var revokedCount = await _userSessionService.RevokeUserSessionsAsync(activeUserSessions, request.UserId, 
                $"LogoutAll: Cierre de todas las sesiones de usaurio ejecutado por el usuario administrador {currentUserId}.", currentUserId, utcNow, asTracking, 
                cancellationToken);
            _logger.LogInformation("LogoutAllCommand: {Count} sesión(es) de usaurio revocada(s) exitosamente. UserId={UserId}, RevocadoPor={CurrentUserId}", revokedCount, 
                request.UserId, currentUserId);
            return Result<LogoutAllResponse>.Ok(new LogoutAllResponse(revokedCount));
        }
    }
}