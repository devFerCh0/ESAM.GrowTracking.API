using ESAM.GrowTracking.API.Validations;
using ESAM.GrowTracking.Application.Validations;
using FluentValidation;

namespace ESAM.GrowTracking.API.Controllers.UserDevices.UnlockUserDevice
{
    public class UnlockUserDeviceRequestValidator : AbstractValidator<UnlockUserDeviceRequest>
    {
        public UnlockUserDeviceRequestValidator()
        {
            RuleFor(uudr => uudr.UserId).Cascade(CascadeMode.Stop)
                .NotNull().WithMessage(RequestValidationMessages.UserIdRequired);
            RuleFor(uudr => uudr.UserDeviceId).Cascade(CascadeMode.Stop)
                .NotNull().WithMessage(CommandValidationMessages.UserDeviceIdRequired);
        }
    }
}