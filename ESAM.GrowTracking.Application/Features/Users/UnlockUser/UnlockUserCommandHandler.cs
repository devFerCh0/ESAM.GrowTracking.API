using ESAM.GrowTracking.Application.Abstractions.Services;
using ESAM.GrowTracking.Application.Extensions;
using ESAM.GrowTracking.Application.Results;
using ESAM.GrowTracking.Application.ValueObjects;
using ESAM.GrowTracking.Domain.Abstractions.DataAccess;
using ESAM.GrowTracking.Domain.Abstractions.DataAccess.Repositories;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ESAM.GrowTracking.Application.Features.Users.UnlockUser
{
    public class UnlockUserCommandHandler : IRequestHandler<UnlockUserCommand, Result>
    {
        private readonly ILogger<UnlockUserCommandHandler> _logger;
        private readonly IValidator<UnlockUserCommand> _validator;
        private readonly IAccessTokenClaimsValidatorService _accessTokenClaimsValidatorService;
        private readonly IUserRepository _userRepository;
        private readonly IDateTimeService _dateTimeService;
        private readonly IUnitOfWork _unitOfWork;

        public UnlockUserCommandHandler(ILogger<UnlockUserCommandHandler> logger, IValidator<UnlockUserCommand> validator, 
            IAccessTokenClaimsValidatorService accessTokenClaimsValidatorService, IUserRepository userRepository, IDateTimeService dateTimeService, IUnitOfWork unitOfWork)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(validator);
            ArgumentNullException.ThrowIfNull(accessTokenClaimsValidatorService);
            ArgumentNullException.ThrowIfNull(userRepository);
            ArgumentNullException.ThrowIfNull(dateTimeService);
            ArgumentNullException.ThrowIfNull(unitOfWork);
            _logger = logger;
            _validator = validator;
            _accessTokenClaimsValidatorService = accessTokenClaimsValidatorService;
            _userRepository = userRepository;
            _dateTimeService = dateTimeService;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(UnlockUserCommand request, CancellationToken cancellationToken)
        {
            var validation = await _validator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                _logger.LogWarning("UnlockUserCommand: validación fallida. Errores: {Errors}", string.Join(" | ", validation.Errors.Select(e => e.ErrorMessage)));
                return Result.Fail(validation.ToCommandErrors());
            }
            var currentUserId = _accessTokenClaimsValidatorService.CurrentUserId;
            if (request.UserId == currentUserId)
            {
                _logger.LogWarning("UnlockUserCommand: intento de auto-desbloqueo denegado. UserId={UserId}", currentUserId);
                return Result.Fail(Error.BusinessRule("No puedes desbloquearte a ti mismo."));
            }
            var asTracking = false;
            var user = await _userRepository.GetByIdAsync(request.UserId, asTracking, cancellationToken);
            if (user is null || user.IsDeleted)
            {
                _logger.LogWarning("UnlockUserCommand: usuario objetivo no encontrado o eliminado. UserId={UserId}", request.UserId);
                return Result.Fail(Error.NotFound("El usuario a desbloquear no existe o se encuentra eliminado."));
            }
            var utcNow = _dateTimeService.UtcNow;
            if (!user.IsLocked(utcNow))
            {
                _logger.LogWarning("UnlockUserCommand: el usuario objetivo no se encuentra bloqueado. TargetUserId={TargetUserId}", request.UserId);
                return Result.Fail(Error.BusinessRule("El usuario no se encuentra bloqueado."));
            }
            user.Unlock(currentUserId, utcNow);
            await _unitOfWork.Users.InsertAsync(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Ok();
        }
    }
}