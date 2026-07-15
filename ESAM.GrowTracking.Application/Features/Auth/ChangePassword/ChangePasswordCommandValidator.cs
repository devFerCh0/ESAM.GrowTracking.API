using ESAM.GrowTracking.Application.Validations;
using FluentValidation;

namespace ESAM.GrowTracking.Application.Features.Auth.ChangePassword
{
    public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
    {
        public ChangePasswordCommandValidator()
        {
            RuleFor(cpc => cpc.CurrentPassword).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage(CommandValidationMessages.CurrentPasswordRequired)
                .MinimumLength(5).WithMessage(CommandValidationMessages.CurrentPasswordMinLength)
                .MaximumLength(100).WithMessage(CommandValidationMessages.CurrentPasswordMaxLength);
            RuleFor(cpc => cpc.NewPassword).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage(CommandValidationMessages.NewPasswordRequired)
                .MinimumLength(5).WithMessage(CommandValidationMessages.NewPasswordMinLength)
                .MaximumLength(100).WithMessage(CommandValidationMessages.NewPasswordMaxLength)
                .NotEqual(cpc => cpc.CurrentPassword).WithMessage(CommandValidationMessages.NewPasswordSameAsCurrent);
            RuleFor(cpc => cpc.ConfirmNewPassword).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage(CommandValidationMessages.ConfirmNewPasswordRequired)
                .Equal(cpc => cpc.NewPassword).WithMessage(CommandValidationMessages.ConfirmNewPasswordMismatch);
        }
    }
}