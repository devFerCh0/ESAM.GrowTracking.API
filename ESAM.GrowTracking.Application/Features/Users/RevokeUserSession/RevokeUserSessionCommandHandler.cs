using ESAM.GrowTracking.Application.Abstractions.Services;
using ESAM.GrowTracking.Application.Extensions;
using ESAM.GrowTracking.Application.Results;
using ESAM.GrowTracking.Application.ValueObjects;
using ESAM.GrowTracking.Domain.Abstractions.DataAccess.Repositories;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ESAM.GrowTracking.Application.Features.Users.RevokeUserSession
{
    public class RevokeUserSessionCommandHandler : IRequestHandler<RevokeUserSessionCommand, Result>
    {
        private readonly ILogger<RevokeUserSessionCommandHandler> _logger;
        private readonly IValidator<RevokeUserSessionCommand> _validator;
        private readonly IAccessTokenClaimsValidatorService _accessTokenClaimsValidatorService;
        private readonly IUserSessionRepository _userSessionRepository;
        private readonly IUserSessionService _userSessionService;
        private readonly IDateTimeService _dateTimeService;

        public RevokeUserSessionCommandHandler(ILogger<RevokeUserSessionCommandHandler> logger, IValidator<RevokeUserSessionCommand> validator,
            IAccessTokenClaimsValidatorService accessTokenClaimsValidatorService, IUserSessionRepository userSessionRepository, IUserSessionService userSessionService,
            IDateTimeService dateTimeService)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(validator);
            ArgumentNullException.ThrowIfNull(accessTokenClaimsValidatorService);
            ArgumentNullException.ThrowIfNull(userSessionRepository);
            ArgumentNullException.ThrowIfNull(userSessionService);
            ArgumentNullException.ThrowIfNull(dateTimeService);
            _logger = logger;
            _validator = validator;
            _accessTokenClaimsValidatorService = accessTokenClaimsValidatorService;
            _userSessionRepository = userSessionRepository;
            _userSessionService = userSessionService;
            _dateTimeService = dateTimeService;
        }

        public async Task<Result> Handle(RevokeUserSessionCommand request, CancellationToken cancellationToken)
        {
            var asTracking = false;
            var validation = await _validator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                _logger.LogWarning("RevokeUserSessionCommand: validación fallida. Errores: {Errors}", string.Join(" | ", validation.Errors.Select(e => e.ErrorMessage)));
                return Result.Fail(validation.ToCommandErrors());
            }
            var userSession = await _userSessionRepository.GetByIdAndUserIdAsync(request.UserSessionId, request.UserId, asTracking, cancellationToken);
            if (userSession is null)
            {
                _logger.LogWarning("RevokeUserSessionCommand: sesión de usuario no encontrada o no pertenece al usuario especificado. UserSessionId={UserSessionId}, " + 
                    "UserId={UserId}", request.UserSessionId, request.UserId);
                return Result.Fail(Error.NotFound("La sesión de usuario especificada no existe o no pertenece al usuario indicado."));
            }
            if (userSession.IsRevoked)
            {
                _logger.LogInformation("RevokeUserSessionCommand: la sesión de usuario ya se encontraba revocada. UserSessionId={UserSessionId}, UserId={UserId}", 
                    request.UserSessionId, request.UserId);
                return Result.Fail(Error.BusinessRule("La sesión de usuario indicada ya se encuentra revocada."));
            }
            var currentUserId = _accessTokenClaimsValidatorService.CurrentUserId;
            var utcNow = _dateTimeService.UtcNow;
            await _userSessionService.RevokeUserSessionAsync(userSession, $"RevokeSession: Revocación de sesión de usuario ejecutada por el usuario administrador {currentUserId}.", 
                currentUserId, utcNow, asTracking, cancellationToken);
            _logger.LogInformation("RevokeUserSessionCommand: sesión de usuario revocada exitosamente. UserSessionId={UserSessionId}, UserId={UserId}, " + 
                "Revocado por={CurrentUserId}", request.UserSessionId, request.UserId, currentUserId);
            return Result.Ok();
        }
    }
}