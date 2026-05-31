using ESAM.GrowTracking.Application.Validations;
using FluentValidation;

namespace ESAM.GrowTracking.Application.Features.Auth.Logout
{
    public class LogoutCommandValidator : AbstractValidator<LogoutCommand>
    {
        public LogoutCommandValidator()
        {
            RuleFor(lc => lc.RefreshTokenRaw).Cascade(CascadeMode.Stop)
                .MinimumLength(3).WithMessage(ValidationMessages.RefreshTokenMinLength)
                .MaximumLength(256).WithMessage(ValidationMessages.RefreshTokenMaxLength)
                .When(lc => !string.IsNullOrWhiteSpace(lc.RefreshTokenRaw));
            RuleFor(lc => lc.DeviceIdentifier).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage(ValidationMessages.DeviceIdentifierRequired)
                .MinimumLength(3).WithMessage(ValidationMessages.DeviceIdentifierMinLength)
                .MaximumLength(256).WithMessage(ValidationMessages.DeviceIdentifierMaxLength)
                .Must(ValidationRules.IsValidGuid).WithMessage(ValidationMessages.DeviceIdentifierInvalid);
        }
    }
}