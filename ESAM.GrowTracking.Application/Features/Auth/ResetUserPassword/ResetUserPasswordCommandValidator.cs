using ESAM.GrowTracking.Application.Validations;
using FluentValidation;

namespace ESAM.GrowTracking.Application.Features.Auth.ResetUserPassword
{
    public class ResetUserPasswordCommandValidator : AbstractValidator<ResetUserPasswordCommand>
    {
        public ResetUserPasswordCommandValidator()
        {
            RuleFor(rupc => rupc.UserId).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage(CommandValidationMessages.UserIdRequired)
                .GreaterThan(0).WithMessage(CommandValidationMessages.UserIdInvalid);
            RuleFor(rupc => rupc.NewPassword).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage(CommandValidationMessages.NewPasswordRequired)
                .MinimumLength(5).WithMessage(CommandValidationMessages.NewPasswordMinLength)
                .MaximumLength(100).WithMessage(CommandValidationMessages.NewPasswordMaxLength);
            RuleFor(rupc => rupc.ConfirmNewPassword).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage(CommandValidationMessages.ConfirmNewPasswordRequired)
                .Equal(rupc => rupc.NewPassword).WithMessage(CommandValidationMessages.ConfirmNewPasswordMismatch);
        }
    }
}