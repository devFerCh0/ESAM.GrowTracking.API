using ESAM.GrowTracking.Application.Validations;
using FluentValidation;

namespace ESAM.GrowTracking.Application.Features.UserSessions.CloseUserSession
{
    public class CloseUserSessionCommandValidator : AbstractValidator<CloseUserSessionCommand>
    {
        public CloseUserSessionCommandValidator()
        {
            RuleFor(cusc => cusc.UserSessionId).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage(CommandValidationMessages.UserSessionIdRequired)
                .GreaterThan(0).WithMessage(CommandValidationMessages.UserSessionIdInvalid);
            RuleFor(cusc => cusc.UserId).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage(CommandValidationMessages.UserIdRequired)
                .GreaterThan(0).WithMessage(CommandValidationMessages.UserIdInvalid);
        }
    }
}