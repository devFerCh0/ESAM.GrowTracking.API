using ESAM.GrowTracking.Application.Validations;
using FluentValidation;

namespace ESAM.GrowTracking.Application.Features.Auth.ChangeWorkProfile
{
    public class ChangeWorkProfileCommandValidator : AbstractValidator<ChangeWorkProfileCommand>
    {
        public ChangeWorkProfileCommandValidator()
        {
            RuleFor(awpc => awpc.WorkProfileId).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage(CommandValidationMessages.WorkProfileIdRequired)
                .GreaterThan(0).WithMessage(CommandValidationMessages.WorkProfileIdInvalid);
        }
    }
}