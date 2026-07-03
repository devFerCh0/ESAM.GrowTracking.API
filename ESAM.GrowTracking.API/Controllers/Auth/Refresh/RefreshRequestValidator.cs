using ESAM.GrowTracking.API.Validations;
using FluentValidation;

namespace ESAM.GrowTracking.API.Controllers.Auth.Refresh
{
    public class RefreshRequestValidator : AbstractValidator<RefreshRequest>
    {
        public RefreshRequestValidator()
        {
            RuleFor(rc => rc.AccessToken).Cascade(CascadeMode.Stop)
                .NotNull().WithMessage(RequestValidationMessages.AccessTokenRequired);
        }
    }
}