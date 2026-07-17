using ESAM.GrowTracking.API.Validations;
using FluentValidation;

namespace ESAM.GrowTracking.API.Controllers.Auth.UnlockUserDevice
{
    public class UnlockUserDeviceRequestValidator : AbstractValidator<UnlockUserDeviceRequest>
    {
        public UnlockUserDeviceRequestValidator()
        {
            RuleFor(uudr => uudr.UserId).Cascade(CascadeMode.Stop)
                .NotNull().WithMessage(RequestValidationMessages.UserIdRequired);
            RuleFor(uudr => uudr.UserDeviceId).Cascade(CascadeMode.Stop)
                .NotNull().WithMessage(RequestValidationMessages.UserDeviceIdRequired);
        }
    }
}