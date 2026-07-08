using ESAM.GrowTracking.API.Validations;
using FluentValidation;

namespace ESAM.GrowTracking.API.Controllers.Auth.RevokeCurrentUserSession
{
    public class RevokeCurrentUserSessionRequestValidator : AbstractValidator<RevokeCurrentUserSessionRequest>
    {
        public RevokeCurrentUserSessionRequestValidator()
        {
            RuleFor(rcusr => rcusr.UserSessionId).Cascade(CascadeMode.Stop)
                .NotNull().WithMessage(RequestValidationMessages.UserSessionIdRequired);
        }
    }
}