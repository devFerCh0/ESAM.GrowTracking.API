using ESAM.GrowTracking.API.Validations;
using FluentValidation;

namespace ESAM.GrowTracking.API.Controllers.Auth.ChangePassword
{
    public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
    {
        public ChangePasswordRequestValidator()
        {
            RuleFor(cpr => cpr.CurrentPassword).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage(RequestValidationMessages.CurrentPasswordRequired);
            RuleFor(cpr => cpr.NewPassword).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage(RequestValidationMessages.NewPasswordRequired);
            RuleFor(cpr => cpr.ConfirmNewPassword).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage(RequestValidationMessages.ConfirmNewPasswordRequired);
        }
    }
}