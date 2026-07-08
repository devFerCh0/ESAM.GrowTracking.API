using ESAM.GrowTracking.API.Validations;
using FluentValidation;

namespace ESAM.GrowTracking.API.Controllers.Auth.LogoutAll
{
    public class LogoutAllRequestValidator : AbstractValidator<LogoutAllRequest>
    {
        public LogoutAllRequestValidator()
        {
            RuleFor(lac => lac.UserId).Cascade(CascadeMode.Stop)
                .NotNull().WithMessage(RequestValidationMessages.UserIdRequired);
        }
    }
}