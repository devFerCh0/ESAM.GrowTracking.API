using ESAM.GrowTracking.Application.Validations;
using FluentValidation;

namespace ESAM.GrowTracking.Application.Features.Auth.GetLockedUserDevices
{
    public class GetLockedUserDeviceQueryValidator : AbstractValidator<GetLockedUserDeviceQuery>
    {
        public GetLockedUserDeviceQueryValidator()
        {
            RuleFor(gludq => gludq.UserId).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage(CommandValidationMessages.UserIdRequired)
                .GreaterThan(0).WithMessage(CommandValidationMessages.UserIdInvalid);
        }
    }
}