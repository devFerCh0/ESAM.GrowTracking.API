using ESAM.GrowTracking.Application.Validations;
using FluentValidation;

namespace ESAM.GrowTracking.Application.Features.Users.UnlockUser
{
    public class UnlockUserCommandValidator : AbstractValidator<UnlockUserCommand>
    {
        public UnlockUserCommandValidator()
        {
            RuleFor(uuc => uuc.UserId).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage(CommandValidationMessages.UserIdRequired)
                .GreaterThan(0).WithMessage(CommandValidationMessages.UserIdInvalid);
        }
    }
}
