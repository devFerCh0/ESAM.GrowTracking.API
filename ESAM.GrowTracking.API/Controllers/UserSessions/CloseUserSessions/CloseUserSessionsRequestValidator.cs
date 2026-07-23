using ESAM.GrowTracking.API.Validations;
using FluentValidation;

namespace ESAM.GrowTracking.API.Controllers.UserSessions.CloseUserSessions
{
    public class CloseUserSessionsRequestValidator : AbstractValidator<CloseUserSessionsRequest>
    {
        public CloseUserSessionsRequestValidator()
        {
            RuleFor(cusr => cusr.UserId).Cascade(CascadeMode.Stop)
                .NotNull().WithMessage(RequestValidationMessages.UserIdRequired);
        }
    }
}