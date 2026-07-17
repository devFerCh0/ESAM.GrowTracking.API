using ESAM.GrowTracking.Application.Abstractions.Services;
using ESAM.GrowTracking.Application.Extensions;
using ESAM.GrowTracking.Application.Results;
using ESAM.GrowTracking.Application.ValueObjects;
using ESAM.GrowTracking.Domain.Abstractions.DataAccess.Repositories;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ESAM.GrowTracking.Application.Features.Users.LockUser
{
    public class LockUserCommandHandler : IRequestHandler<LockUserCommand, Result>
    {
        private readonly ILogger<LockUserCommandHandler> _logger;
        private readonly IValidator<LockUserCommand> _validator;
        private readonly IAccessTokenClaimsValidatorService _accessTokenClaimsValidatorService;
        private readonly IUserRepository _userRepository;
        private readonly IDateTimeService _dateTimeService;
        private readonly IUserService _userService;

        public LockUserCommandHandler(ILogger<LockUserCommandHandler> logger, IValidator<LockUserCommand> validator, 
            IAccessTokenClaimsValidatorService accessTokenClaimsValidatorService, IUserRepository userRepository, IDateTimeService dateTimeService, IUserService userService)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(validator);
            ArgumentNullException.ThrowIfNull(accessTokenClaimsValidatorService);
            ArgumentNullException.ThrowIfNull(userRepository);
            ArgumentNullException.ThrowIfNull(dateTimeService);
            ArgumentNullException.ThrowIfNull(userService);
            _logger = logger;
            _validator = validator;
            _accessTokenClaimsValidatorService = accessTokenClaimsValidatorService;
            _userRepository = userRepository;
            _dateTimeService = dateTimeService;
            _userService = userService;
        }

        public async Task<Result> Handle(LockUserCommand request, CancellationToken cancellationToken)
        {
            var validation = await _validator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                _logger.LogWarning("LockUserCommand: validación fallida. Errores: {Errors}", string.Join(" | ", validation.Errors.Select(e => e.ErrorMessage)));
                return Result.Fail(validation.ToCommandErrors());
            }
            var currentUserId = _accessTokenClaimsValidatorService.CurrentUserId;
            if (request.UserId == currentUserId)
            {
                _logger.LogWarning("LockUserCommand: intento de auto-bloqueo denegado. UserId={UserId}", currentUserId);
                return Result.Fail(Error.BusinessRule("No es posible bloquear la propia cuenta."));
            }
            var asTracking = false;
            var user = await _userRepository.GetByIdAsync(request.UserId, asTracking, cancellationToken);
            if (user is null || user.IsDeleted)
            {
                _logger.LogWarning("LockUserCommand: usuario objetivo no encontrado o eliminado. UserId={UserId}", request.UserId);
                return Result.Fail(Error.NotFound("El usuario a bloquear no existe o se encuentra eliminado."));
            }
            var utcNow = _dateTimeService.UtcNow;
            if (user.IsLocked(utcNow))
            {
                _logger.LogWarning("LockUserCommand: el usuario objetivo ya se encuentra bloqueado. UserId={UserId}, BloqueadoHasta={LockoutEndAt}", request.UserId, 
                    user.LockoutEndAt);
                return Result.Fail(Error.BusinessRule("El usuario ya se encuentra bloqueado."));
            }
            user.Lock(request.LockoutEndAt, currentUserId, utcNow);
            user.UpdateSecurityCredentials(Guid.NewGuid().ToString(), user.TokenVersion + 1, currentUserId, utcNow);
            await _userService.LockUserAsync(user, "LockUser: Sesión finalizada por bloqueo administrativo de la cuenta.", currentUserId, utcNow, asTracking, cancellationToken);
            return Result.Ok();
        }
    }
}