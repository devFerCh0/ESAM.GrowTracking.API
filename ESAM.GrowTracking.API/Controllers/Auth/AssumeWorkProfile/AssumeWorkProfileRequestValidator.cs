using ESAM.GrowTracking.API.Validations;
using FluentValidation;

namespace ESAM.GrowTracking.API.Controllers.Auth.AssumeWorkProfile
{
    public class AssumeWorkProfileRequestValidator : AbstractValidator<AssumeWorkProfileRequest>
    {
        public AssumeWorkProfileRequestValidator()
        {
            RuleFor(awpc => awpc.WorkProfileId).Cascade(CascadeMode.Stop)
                .NotNull().WithMessage(RequestValidationMessages.WorkProfileIdRequired);
        }
    }
}