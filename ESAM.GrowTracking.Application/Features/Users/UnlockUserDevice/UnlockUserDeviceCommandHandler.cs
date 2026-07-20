using ESAM.GrowTracking.Application.Abstractions.Services;
using ESAM.GrowTracking.Application.Extensions;
using ESAM.GrowTracking.Application.Results;
using ESAM.GrowTracking.Application.ValueObjects;
using ESAM.GrowTracking.Domain.Abstractions.DataAccess;
using ESAM.GrowTracking.Domain.Abstractions.DataAccess.Repositories;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ESAM.GrowTracking.Application.Features.Users.UnlockUserDevice
{
    public class UnlockUserDeviceCommandHandler : IRequestHandler<UnlockUserDeviceCommand, Result>
    {
        private readonly ILogger<UnlockUserDeviceCommandHandler> _logger;
        private readonly IValidator<UnlockUserDeviceCommand> _validator;
        private readonly IUserDeviceRepository _userDeviceRepository;
        private readonly IDateTimeService _dateTimeService;
        private readonly IAccessTokenClaimsValidatorService _accessTokenClaimsValidatorService;
        private readonly IUnitOfWork _unitOfWork;

        public UnlockUserDeviceCommandHandler(ILogger<UnlockUserDeviceCommandHandler> logger, IValidator<UnlockUserDeviceCommand> validator, 
            IUserDeviceRepository userDeviceRepository, IDateTimeService dateTimeService, IAccessTokenClaimsValidatorService accessTokenClaimsValidatorService, 
            IUnitOfWork unitOfWork)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(validator);
            ArgumentNullException.ThrowIfNull(userDeviceRepository);
            ArgumentNullException.ThrowIfNull(dateTimeService);
            ArgumentNullException.ThrowIfNull(accessTokenClaimsValidatorService);
            ArgumentNullException.ThrowIfNull(unitOfWork);
            _logger = logger;
            _validator = validator;
            _userDeviceRepository = userDeviceRepository;
            _dateTimeService = dateTimeService;
            _accessTokenClaimsValidatorService = accessTokenClaimsValidatorService;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(UnlockUserDeviceCommand request, CancellationToken cancellationToken)
        {
            var validation = await _validator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                _logger.LogWarning("UnlockUserDeviceCommand: validación fallida. Errores: {Errors}", string.Join(" | ", validation.Errors.Select(e => e.ErrorMessage)));
                return Result.Fail(validation.ToCommandErrors());
            }
            var asTracking = false;
            var userDevice = await _userDeviceRepository.GetByIdAsync(request.UserDeviceId, asTracking, cancellationToken);
            if (userDevice is null || userDevice.IsDeleted || userDevice.UserId != request.UserId)
            {
                _logger.LogWarning("UnlockUserDeviceCommand: dispositivo de usuario no encontrado, eliminado o no perteneciente al usuario indicado. UserId={UserId}, " + 
                    "UserDeviceId={UserDeviceId}", request.UserId, request.UserDeviceId);
                return Result.Fail(Error.NotFound("El dispositivo de usuario indicado no fue encontrado para el usuario especificado."));
            }
            var utcNow = _dateTimeService.UtcNow;
            if (!userDevice.IsLocked(utcNow))
            {
                _logger.LogWarning("UnlockUserDeviceCommand: el dispositivo de usuario no se encuentra bloqueado actualmente. UserId={UserId}, UserDeviceId={UserDeviceId}", 
                    request.UserId, request.UserDeviceId);
                return Result.Fail(Error.BusinessRule("El dispositivo de usuario no se encuentra bloqueado actualmente."));
            }
            var currentUserId = _accessTokenClaimsValidatorService.CurrentUserId;
            userDevice.ResetFailedLogin(currentUserId, utcNow);
            userDevice.UpdateLastSeenAt(utcNow, currentUserId, utcNow);
            await _unitOfWork.UserDevices.UpdateAsync(userDevice, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("UnlockUserDeviceCommand: dispositivo de usuario desbloqueado exitosamente. UserId={UserId}, UserDeviceId={UserDeviceId}, " +
                "CurrentUserId={CurrentUserId}", request.UserId, request.UserDeviceId, currentUserId);
            return Result.Ok();
        }
    }
}