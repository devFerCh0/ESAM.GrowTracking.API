using ESAM.GrowTracking.Application.Validations;
using FluentValidation;

namespace ESAM.GrowTracking.Application.Features.UserSessions.CloseUserSessions
{
    public class CloseUserSessionsCommandValidator : AbstractValidator<CloseUserSessionsCommand>
    {
        public CloseUserSessionsCommandValidator()
        {
            RuleFor(cus => cus.UserId).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage(CommandValidationMessages.UserIdRequired)
                .GreaterThan(0).WithMessage(CommandValidationMessages.UserIdInvalid);
        }
    }
}