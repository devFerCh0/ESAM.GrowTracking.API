using ESAM.GrowTracking.Application.Abstractions.Services;
using ESAM.GrowTracking.Application.Extensions;
using ESAM.GrowTracking.Application.Results;
using ESAM.GrowTracking.Application.ValueObjects;
using ESAM.GrowTracking.Domain.Abstractions.DataAccess;
using ESAM.GrowTracking.Domain.Abstractions.DataAccess.Repositories;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ESAM.GrowTracking.Application.Features.UserDevices.UnlockUserDevice
{
    public class UnlockUserDeviceCommandHandler : IRequestHandler<UnlockUserDeviceCommand, Result>
    {
        private readonly ILogger<UnlockUserDeviceCommandHandler> _logger;
        private readonly IValidator<UnlockUserDeviceCommand> _validator;
        private readonly IUserRepository _userRepository;
        private readonly IUserDeviceRepository _userDeviceRepository;
        private readonly IDateTimeService _dateTimeService;
        private readonly IAccessTokenClaimsValidatorService _accessTokenClaimsValidatorService;
        private readonly IUserDeviceService _userDeviceService;
        private readonly IUnitOfWork _unitOfWork;

        public UnlockUserDeviceCommandHandler(ILogger<UnlockUserDeviceCommandHandler> logger, IValidator<UnlockUserDeviceCommand> validator, IUserRepository userRepository,
            IUserDeviceRepository userDeviceRepository, IDateTimeService dateTimeService, IAccessTokenClaimsValidatorService accessTokenClaimsValidatorService,
            IUserDeviceService userDeviceService, IUnitOfWork unitOfWork)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(validator);
            ArgumentNullException.ThrowIfNull(userRepository);
            ArgumentNullException.ThrowIfNull(userDeviceRepository);
            ArgumentNullException.ThrowIfNull(dateTimeService);
            ArgumentNullException.ThrowIfNull(accessTokenClaimsValidatorService);
            ArgumentNullException.ThrowIfNull(userDeviceService);
            ArgumentNullException.ThrowIfNull(unitOfWork);
            _logger = logger;
            _validator = validator;
            _userRepository = userRepository;
            _userDeviceRepository = userDeviceRepository;
            _dateTimeService = dateTimeService;
            _accessTokenClaimsValidatorService = accessTokenClaimsValidatorService;
            _userDeviceService = userDeviceService;
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
            var isUserValid = await _userRepository.IsUserValidAsync(request.UserId, asTracking, cancellationToken);
            if (!isUserValid)
            {
                _logger.LogWarning("GetUserSessionsQuery: usuario no encontrado o eliminado. UserId={UserId}", request.UserId);
                return Result.Fail(Error.NotFound("No se encontró el usuario especificado."));
            }
            var userDevice = await _userDeviceRepository.GetByIdAndUserIdAsync(request.UserDeviceId, request.UserId, asTracking, cancellationToken);
            if (userDevice is null || userDevice.IsDeleted)
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
            _userDeviceService.UserDeviceUnlock(userDevice, currentUserId, utcNow);
            await _unitOfWork.UserDevices.UpdateAsync(userDevice, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Ok();
        }
    }
}