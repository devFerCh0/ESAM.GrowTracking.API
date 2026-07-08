using ESAM.GrowTracking.Application.Abstractions.Services;
using ESAM.GrowTracking.Application.Extensions;
using ESAM.GrowTracking.Application.Results;
using ESAM.GrowTracking.Application.ValueObjects;
using ESAM.GrowTracking.Domain.Abstractions.DataAccess.Repositories;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ESAM.GrowTracking.Application.Features.Auth.RevokeCurrentUserSession
{
    public class RevokeCurrentUserSessionCommandHandler : IRequestHandler<RevokeCurrentUserSessionCommand, Result>
    {
        private readonly ILogger<RevokeCurrentUserSessionCommandHandler> _logger;
        private readonly IValidator<RevokeCurrentUserSessionCommand> _validator;
        private readonly IAccessTokenClaimsValidatorService _accessTokenClaimsValidatorService;
        private readonly IUserSessionRepository _userSessionRepository;
        private readonly IUserSessionService _userSessionService;
        private readonly IDateTimeService _dateTimeService;

        public RevokeCurrentUserSessionCommandHandler(ILogger<RevokeCurrentUserSessionCommandHandler> logger, IValidator<RevokeCurrentUserSessionCommand> validator,
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

        public async Task<Result> Handle(RevokeCurrentUserSessionCommand request, CancellationToken cancellationToken)
        {
            var asTracking = false;
            var validation = await _validator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                _logger.LogWarning("RevokeCurrentUserSessionCommand: validación fallida. Errores: {Errors}", string.Join(" | ", validation.Errors.Select(e => e.ErrorMessage)));
                return Result.Fail(validation.ToCommandErrors());
            }
            var currentUserId = _accessTokenClaimsValidatorService.CurrentUserId;
            var userSession = await _userSessionRepository.GetByIdAndUserIdAsync(request.UserSessionId, currentUserId, asTracking, cancellationToken);
            if (userSession is null)
            {
                _logger.LogWarning("RevokeCurrentUserSessionCommand: sesión de usuario no encontrada o no te pertenece. UserSessionId={UserSessionId}, UserId={UserId}", 
                    request.UserSessionId, currentUserId);
                return Result.Fail(Error.NotFound("La sesión de usuario especificada no existe o no te pertenece."));
            }
            if (userSession.IsRevoked)
            {
                _logger.LogInformation("RevokeCurrentUserSessionCommand: la sesión de usuario ya se encontraba revocada. UserSessionId={UserSessionId}, UserId={UserId}", 
                    request.UserSessionId, currentUserId);
                return Result.Fail(Error.BusinessRule("La sesión de usuario indicada ya se encuentra revocada."));
            }
            var utcNow = _dateTimeService.UtcNow;
            await _userSessionService.RevokeUserSessionAsync(userSession, $"RevokeSession: Revocación manual de sesión realizada por el propio usuario {currentUserId}.",
                currentUserId, utcNow, asTracking, cancellationToken);
            _logger.LogInformation("RevokeCurrentUserSessionCommand: sesión de usuario revocada exitosamente. UserSessionId={UserSessionId}, UserId={UserId}", 
                request.UserSessionId, currentUserId);
            return Result.Ok();
        }
    }
}