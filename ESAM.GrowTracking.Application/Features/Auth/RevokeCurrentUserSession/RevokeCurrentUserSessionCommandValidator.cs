using ESAM.GrowTracking.Application.Validations;
using FluentValidation;

namespace ESAM.GrowTracking.Application.Features.Auth.RevokeCurrentUserSession
{
    public class RevokeCurrentUserSessionCommandValidator : AbstractValidator<RevokeCurrentUserSessionCommand>
    {
        public RevokeCurrentUserSessionCommandValidator()
        {
            RuleFor(rcusc => rcusc.UserSessionId).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage(CommandValidationMessages.UserSessionIdRequired)
                .GreaterThan(0).WithMessage(CommandValidationMessages.UserSessionIdInvalid);
        }
    }
}