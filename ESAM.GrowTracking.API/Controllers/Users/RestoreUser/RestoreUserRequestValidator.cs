using ESAM.GrowTracking.API.Validations;
using FluentValidation;

namespace ESAM.GrowTracking.API.Controllers.Users.RestoreUser
{
    public class RestoreUserRequestValidator : AbstractValidator<RestoreUserRequest>
    {
        public RestoreUserRequestValidator()
        {
            RuleFor(rur => rur.UserId).Cascade(CascadeMode.Stop)
                .NotNull().WithMessage(RequestValidationMessages.UserIdRequired);
        }
    }
}