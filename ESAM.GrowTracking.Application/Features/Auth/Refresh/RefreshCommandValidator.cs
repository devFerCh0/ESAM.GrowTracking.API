using ESAM.GrowTracking.Application.Validations;
using FluentValidation;

namespace ESAM.GrowTracking.Application.Features.Auth.Refresh
{
    public class RefreshCommandValidator : AbstractValidator<RefreshCommand>
    {
        public RefreshCommandValidator()
        {
            RuleFor(rc => rc.RefreshTokenRaw).Cascade(CascadeMode.Stop)
                .MinimumLength(3).WithMessage(CommandValidationMessages.RefreshTokenMinLength)
                .MaximumLength(256).WithMessage(CommandValidationMessages.RefreshTokenMaxLength)
                .When(rc => !string.IsNullOrWhiteSpace(rc.RefreshTokenRaw));
            RuleFor(rc => rc.DeviceIdentifier).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage(ValidationMessages.DeviceIdentifierRequired)
                .MinimumLength(3).WithMessage(ValidationMessages.DeviceIdentifierMinLength)
                .MaximumLength(256).WithMessage(ValidationMessages.DeviceIdentifierMaxLength)
                .Must(ValidationRules.IsValidGuid).WithMessage(ValidationMessages.DeviceIdentifierInvalid);
        }
    }
}