using ESAM.GrowTracking.Application.Features.Auth.UnlockUserAccount;
using ESAM.GrowTracking.Application.Validations;
using FluentValidation;

namespace ESAM.GrowTracking.Application.Features.Auth.UnlockUserDevice
{
    public class UnlockUserDeviceCommandValidator : AbstractValidator<UnlockUserDeviceCommand>
    {
        public UnlockUserDeviceCommandValidator()
        {
            RuleFor(uudc => uudc.UserId).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage(CommandValidationMessages.UserIdRequired)
                .GreaterThan(0).WithMessage(CommandValidationMessages.UserIdInvalid);
            RuleFor(uudc => uudc.UserDeviceId).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage(CommandValidationMessages.UserDeviceIdRequired)
                .GreaterThan(0).WithMessage(CommandValidationMessages.UserDeviceIdInvalid);
        }
    }
}