using ESAM.GrowTracking.Application.Validations;
using FluentValidation;

namespace ESAM.GrowTracking.Application.Features.Auth.Login
{
    public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
    {
        public LoginCommandValidator()
        {
            RuleFor(lc => lc.Credential).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage(ValidationMessages.CredentialRequired)
                .MinimumLength(5).WithMessage(ValidationMessages.CredentialMinLength)
                .MaximumLength(50).WithMessage(ValidationMessages.CredentialMaxLength)
                .Must(ValidationRules.IsValidCredential).WithMessage((_, credential) => credential!.Contains('@')
                    ? ValidationMessages.CredentialValidatedEmail : ValidationMessages.CredentialValidatedUsername);
            RuleFor(lc => lc.Password).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage(ValidationMessages.PasswordRequired)
                .MinimumLength(5).WithMessage(ValidationMessages.PasswordMinLength)
                .MaximumLength(100).WithMessage(ValidationMessages.PasswordMaxLength);
            RuleFor(lc => lc.DeviceIdentifier).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage(ValidationMessages.DeviceIdentifierRequired)
                .MinimumLength(3).WithMessage(ValidationMessages.DeviceIdentifierMinLength)
                .MaximumLength(256).WithMessage(ValidationMessages.DeviceIdentifierMaxLength)
                .Must(ValidationRules.IsValidGuid).WithMessage(ValidationMessages.DeviceIdentifierInvalid);
            RuleFor(lc => lc.DeviceName).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage(ValidationMessages.DeviceNameRequired)
                .MinimumLength(2).WithMessage(ValidationMessages.DeviceNameMinLength)
                .MaximumLength(100).WithMessage(ValidationMessages.DeviceNameMaxLength)
                .Must(ValidationRules.HasNoControlChars).WithMessage(ValidationMessages.DeviceNameInvalid);
            RuleFor(lc => lc.ApiClientType).Cascade(CascadeMode.Stop)
                .IsInEnum().WithMessage(ValidationMessages.ApiClientTypeInvalid);
        }
    }
}