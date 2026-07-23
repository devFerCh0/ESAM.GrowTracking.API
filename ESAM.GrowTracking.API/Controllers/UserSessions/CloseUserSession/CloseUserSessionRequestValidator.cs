using ESAM.GrowTracking.API.Validations;
using FluentValidation;

namespace ESAM.GrowTracking.API.Controllers.UserSessions.CloseUserSession
{
    public class CloseUserSessionRequestValidator : AbstractValidator<CloseUserSessionRequest>
    {
        public CloseUserSessionRequestValidator()
        {
            RuleFor(cusr => cusr.UserSessionId).Cascade(CascadeMode.Stop)
                .NotNull().WithMessage(RequestValidationMessages.UserSessionIdRequired);
            RuleFor(cusr => cusr.UserId).Cascade(CascadeMode.Stop)
                .NotNull().WithMessage(RequestValidationMessages.UserIdRequired);
        }
    }
}