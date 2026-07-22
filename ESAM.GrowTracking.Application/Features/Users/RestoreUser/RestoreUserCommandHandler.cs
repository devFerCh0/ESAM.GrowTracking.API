using ESAM.GrowTracking.Application.Abstractions.Services;
using ESAM.GrowTracking.Application.Extensions;
using ESAM.GrowTracking.Application.Results;
using ESAM.GrowTracking.Application.ValueObjects;
using ESAM.GrowTracking.Domain.Abstractions.DataAccess;
using ESAM.GrowTracking.Domain.Abstractions.DataAccess.Repositories;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ESAM.GrowTracking.Application.Features.Users.RestoreUser
{
    public class RestoreUserCommandHandler : IRequestHandler<RestoreUserCommand, Result>
    {
        private readonly ILogger<RestoreUserCommandHandler> _logger;
        private readonly IValidator<RestoreUserCommand> _validator;
        private readonly IAccessTokenClaimsValidatorService _accessTokenClaimsValidatorService;
        private readonly IUserRepository _userRepository;
        private readonly IDateTimeService _dateTimeService;
        private readonly IUserService _userService;
        private readonly IUnitOfWork _unitOfWork;

        public RestoreUserCommandHandler(ILogger<RestoreUserCommandHandler> logger, IValidator<RestoreUserCommand> validator,
            IAccessTokenClaimsValidatorService accessTokenClaimsValidatorService, IUserRepository userRepository, IDateTimeService dateTimeService, IUserService userService,
            IUnitOfWork unitOfWork)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(validator);
            ArgumentNullException.ThrowIfNull(accessTokenClaimsValidatorService);
            ArgumentNullException.ThrowIfNull(userRepository);
            ArgumentNullException.ThrowIfNull(dateTimeService);
            ArgumentNullException.ThrowIfNull(userService);
            ArgumentNullException.ThrowIfNull(unitOfWork);
            _logger = logger;
            _validator = validator;
            _accessTokenClaimsValidatorService = accessTokenClaimsValidatorService;
            _userRepository = userRepository;
            _dateTimeService = dateTimeService;
            _userService = userService;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(RestoreUserCommand request, CancellationToken cancellationToken)
        {
            var validation = await _validator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                _logger.LogWarning("RestoreUserCommand: validación fallida. Errores: {Errors}", string.Join(" | ", validation.Errors.Select(e => e.ErrorMessage)));
                return Result.Fail(validation.ToCommandErrors());
            }
            var currentUserId = _accessTokenClaimsValidatorService.CurrentUserId;
            if (request.UserId == currentUserId)
            {
                _logger.LogWarning("RestoreUserCommand: intento de restauración de la propia cuenta denegado. UserId={UserId}", currentUserId);
                return Result.Fail(Error.BusinessRule("No es posible restaurar la propia cuenta."));
            }
            var asTracking = false;
            var user = await _userRepository.GetByIdAsync(request.UserId, asTracking, cancellationToken);
            if (user is null)
            {
                _logger.LogWarning("RestoreUserCommand: usuario objetivo no encontrado. UserId={UserId}", request.UserId);
                return Result.Fail(Error.NotFound("El usuario a restaurar no existe."));
            }
            if (!user.IsDeleted)
            {
                _logger.LogWarning("RestoreUserCommand: el usuario objetivo no se encuentra eliminado. UserId={UserId}", request.UserId);
                return Result.Fail(Error.BusinessRule("El usuario no se encuentra eliminado."));
            }
            var utcNow = _dateTimeService.UtcNow;
            _userService.UserRestore(user, currentUserId, utcNow);
            await _unitOfWork.Users.UpdateAsync(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Ok();
        }
    }
}