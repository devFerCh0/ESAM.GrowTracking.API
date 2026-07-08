using ESAM.GrowTracking.Application.Validations;
using FluentValidation;

namespace ESAM.GrowTracking.Application.Features.Auth.GetActiveUserSessions
{
    public class GetActiveUserSessionsQueryValidator : AbstractValidator<GetActiveUserSessionsQuery>
    {
        public GetActiveUserSessionsQueryValidator()
        {
            RuleFor(gausq => gausq.UserId).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage(CommandValidationMessages.UserIdRequired)
                .GreaterThan(0).WithMessage(CommandValidationMessages.UserIdInvalid);
        }
    }
}