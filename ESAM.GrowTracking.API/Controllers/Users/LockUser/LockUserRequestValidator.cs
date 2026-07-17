using ESAM.GrowTracking.API.Validations;
using FluentValidation;

namespace ESAM.GrowTracking.API.Controllers.Users.LockUser
{
    public class LockUserRequestValidator : AbstractValidator<LockUserRequest>
    {
        public LockUserRequestValidator()
        {
            RuleFor(lur => lur.UserId).Cascade(CascadeMode.Stop)
                .NotNull().WithMessage(RequestValidationMessages.UserIdRequired);
            RuleFor(lur => lur.LockoutEndAt).Cascade(CascadeMode.Stop)
                .NotNull().WithMessage(RequestValidationMessages.LockoutEndAtRequired);
        }
    }
}