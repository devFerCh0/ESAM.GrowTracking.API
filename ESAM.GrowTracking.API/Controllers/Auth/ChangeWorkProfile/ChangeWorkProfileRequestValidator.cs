using ESAM.GrowTracking.API.Validations;
using FluentValidation;

namespace ESAM.GrowTracking.API.Controllers.Auth.ChangeWorkProfile
{
    public class ChangeWorkProfileRequestValidator : AbstractValidator<ChangeWorkProfileRequest>
    {
        public ChangeWorkProfileRequestValidator()
        {
            RuleFor(cwpr => cwpr.WorkProfileId).Cascade(CascadeMode.Stop)
                .NotNull().WithMessage(RequestValidationMessages.WorkProfileIdRequired);
        }
    }
}