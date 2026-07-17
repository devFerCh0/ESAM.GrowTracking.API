using ESAM.GrowTracking.Application.Validations;
using FluentValidation;

namespace ESAM.GrowTracking.Application.Features.Users.LockUser
{
    public class LockUserCommandValidator : AbstractValidator<LockUserCommand>
    {
        public LockUserCommandValidator()
        {
            RuleFor(luc => luc.UserId).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage(CommandValidationMessages.UserIdRequired)
                .GreaterThan(0).WithMessage(CommandValidationMessages.UserIdInvalid);
            RuleFor(luc => luc.LockoutEndAt).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage(CommandValidationMessages.LockoutEndAtRequired)
                .Must(lockoutEndAt => lockoutEndAt > DateTime.UtcNow).WithMessage(CommandValidationMessages.LockoutEndAtInvalid);
        }
    }
}