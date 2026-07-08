using ESAM.GrowTracking.API.Validations;
using FluentValidation;

namespace ESAM.GrowTracking.API.Controllers.Auth.RevokeUserSession
{
    public class RevokeUserSessionRequestValidator : AbstractValidator<RevokeUserSessionRequest>
    {
        public RevokeUserSessionRequestValidator()
        {
            RuleFor(rusr => rusr.UserSessionId).Cascade(CascadeMode.Stop)
                .NotNull().WithMessage(RequestValidationMessages.UserSessionIdRequired);
            RuleFor(rusr => rusr.UserId).Cascade(CascadeMode.Stop)
                .NotNull().WithMessage(RequestValidationMessages.UserIdRequired);
        }
    }
}