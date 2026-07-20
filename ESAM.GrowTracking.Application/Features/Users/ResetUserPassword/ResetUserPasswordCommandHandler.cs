using ESAM.GrowTracking.Application.Abstractions.Services;
using ESAM.GrowTracking.Application.Extensions;
using ESAM.GrowTracking.Application.Results;
using ESAM.GrowTracking.Application.ValueObjects;
using ESAM.GrowTracking.Domain.Abstractions.DataAccess.Repositories;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ESAM.GrowTracking.Application.Features.Users.ResetUserPassword
{
    public class ResetUserPasswordCommandHandler : IRequestHandler<ResetUserPasswordCommand, Result>
    {
        private readonly ILogger<ResetUserPasswordCommandHandler> _logger;
        private readonly IValidator<ResetUserPasswordCommand> _validator;
        private readonly IAccessTokenClaimsValidatorService _accessTokenClaimsValidatorService;
        private readonly IUserRepository _userRepository;
        private readonly IHashService _hashService;
        private readonly IDateTimeService _dateTimeService;
        private readonly IUserSessionRepository _userSessionRepository;
        private readonly IUserSessionService _userSessionService;

        public ResetUserPasswordCommandHandler(ILogger<ResetUserPasswordCommandHandler> logger, IValidator<ResetUserPasswordCommand> validator,
            IAccessTokenClaimsValidatorService accessTokenClaimsValidatorService, IUserRepository userRepository, IHashService hashService, IDateTimeService dateTimeService, 
            IUserSessionRepository userSessionRepository, IUserSessionService userSessionService)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(validator);
            ArgumentNullException.ThrowIfNull(accessTokenClaimsValidatorService);
            ArgumentNullException.ThrowIfNull(userRepository);
            ArgumentNullException.ThrowIfNull(hashService);
            ArgumentNullException.ThrowIfNull(dateTimeService);
            ArgumentNullException.ThrowIfNull(userSessionRepository);
            ArgumentNullException.ThrowIfNull(userSessionService);
            _logger = logger;
            _validator = validator;
            _accessTokenClaimsValidatorService = accessTokenClaimsValidatorService;
            _userRepository = userRepository;
            _hashService = hashService;
            _dateTimeService = dateTimeService;
            _userSessionRepository = userSessionRepository;
            _userSessionService = userSessionService;
        }

        public async Task<Result> Handle(ResetUserPasswordCommand request, CancellationToken cancellationToken)
        {
            var asTracking = false;
            var validation = await _validator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                _logger.LogWarning("ResetUserPasswordCommand: validación fallida. Errores: {Errors}", string.Join(" | ", validation.Errors.Select(e => e.ErrorMessage)));
                return Result.Fail(validation.ToCommandErrors());
            }
            var currentUserId = _accessTokenClaimsValidatorService.CurrentUserId;
            if (request.UserId == currentUserId)
            {
                _logger.LogWarning("ResetUserPasswordCommand: intento de autorestablecimiento mediante comando administrativo. UserId={UserId}", currentUserId);
                return Result.Fail(Error.Validation("Para cambiar su propia contraseña utilice la funcionalidad de cambio de contraseña personal."));
            }
            var user = await _userRepository.GetByIdAsync(request.UserId, asTracking, cancellationToken);
            if (user is null || user.IsDeleted)
            {
                _logger.LogWarning("ResetUserPasswordCommand: usuario objetivo no encontrado o eliminado. UserId={UserId}", request.UserId);
                return Result.Fail(Error.NotFound("No se encontró el usuario especificado."));
            }
            var isNewPasswordSameAsCurrent = _hashService.VerifyHash(request.NewPassword, user.Salt, user.PasswordHash);
            if (isNewPasswordSameAsCurrent)
            {
                _logger.LogWarning("ResetUserPasswordCommand: la nueva contraseña coincide con la actual del usuario. UserId={UserId}", request.UserId);
                return Result.Fail(Error.BusinessRule("La nueva contraseña debe ser diferente a la contraseña actual del usuario."));
            }
            var utcNow = _dateTimeService.UtcNow;



            var activeUserSessions = await _userSessionRepository.GetActiveByUserIdAsync(request.UserId, utcNow, asTracking, cancellationToken);
            var activeSessionsCount = await _userSessionService.ResetPassworsAndRevokeUserSessionsAsync(activeUserSessions, user, request.NewPassword, 
                "Restablecimiento administrativo de contraseña: todas las sesiones activas del usuario fueron revocadas.", currentUserId, utcNow, asTracking, cancellationToken);
            _logger.LogInformation("ResetUserPasswordCommand: contraseña restablecida y sesiones revocadas exitosamente. UserId={UserId}, CurrentUserId={CurrentUserId}, " +
                "SesionesRevocadas={SessionsCount}", request.UserId, currentUserId, activeSessionsCount);
            return Result.Ok();
        }
    }
}