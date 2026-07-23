using ESAM.GrowTracking.API.Validations;
using FluentValidation;

namespace ESAM.GrowTracking.API.Controllers.Users.ResetUserPassword
{
    public class ResetUserPasswordRequestValidator : AbstractValidator<ResetUserPasswordRequest>
    {
        public ResetUserPasswordRequestValidator()
        {
            RuleFor(rupr => rupr.UserId).Cascade(CascadeMode.Stop)
                .NotNull().WithMessage(RequestValidationMessages.UserIdRequired);
            RuleFor(rupr => rupr.NewPassword).Cascade(CascadeMode.Stop)
                .NotNull().WithMessage(RequestValidationMessages.NewPasswordRequired);
            RuleFor(rupr => rupr.ConfirmNewPassword).Cascade(CascadeMode.Stop)
                .NotNull().WithMessage(RequestValidationMessages.ConfirmNewPasswordRequired);
        }
    }
}