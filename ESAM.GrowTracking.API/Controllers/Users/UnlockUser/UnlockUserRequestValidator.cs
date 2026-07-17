using ESAM.GrowTracking.API.Validations;
using FluentValidation;

namespace ESAM.GrowTracking.API.Controllers.Users.UnlockUser
{
    public class UnlockUserRequestValidator : AbstractValidator<UnlockUserRequest>
    {
        public UnlockUserRequestValidator()
        {
            RuleFor(lur => lur.UserId).Cascade(CascadeMode.Stop)
                .NotNull().WithMessage(RequestValidationMessages.UserIdRequired);
        }
    }
}