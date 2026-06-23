using ESAM.GrowTracking.Application.Validations;
using FluentValidation;

namespace ESAM.GrowTracking.Application.Features.Auth.AssumeWorkProfile
{
    public class AssumeWorkProfileCommandValidator : AbstractValidator<AssumeWorkProfileCommand>
    {
        public AssumeWorkProfileCommandValidator()
        {
            RuleFor(awpc => awpc.WorkProfileId).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage(CommandValidationMessages.WorkProfileIdRequired)
                .GreaterThan(0).WithMessage(CommandValidationMessages.WorkProfileIdInvalid);
        }
    }
}