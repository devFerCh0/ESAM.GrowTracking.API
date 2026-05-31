using ESAM.GrowTracking.Application.Validations;
using FluentValidation;

namespace ESAM.GrowTracking.Application.Features.Auth.AssumeWorkProfile
{
    public class AssumeWorkProfileCommandValidator : AbstractValidator<AssumeWorkProfileCommand>
    {
        public AssumeWorkProfileCommandValidator()
        {
            RuleFor(awpc => awpc.WorkProfileId).Cascade(CascadeMode.Stop)
                .NotNull().WithMessage(ValidationMessages.WorkProfileIdRequired)
                .GreaterThan(0).WithMessage(ValidationMessages.WorkProfileIdInvalid);
        }
    }
}