using ESAM.GrowTracking.Application.Validations;
using FluentValidation;

namespace ESAM.GrowTracking.Application.Features.Users.RevokeUserSession
{
    public class RevokeUserSessionCommandValidator : AbstractValidator<RevokeUserSessionCommand>
    {
        public RevokeUserSessionCommandValidator()
        {
            RuleFor(rusc => rusc.UserSessionId).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage(CommandValidationMessages.UserSessionIdRequired)
                .GreaterThan(0).WithMessage(CommandValidationMessages.UserSessionIdInvalid);
            RuleFor(rusc => rusc.UserId).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage(CommandValidationMessages.UserIdRequired)
                .GreaterThan(0).WithMessage(CommandValidationMessages.UserIdInvalid);
        }
    }
}