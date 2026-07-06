using ESAM.GrowTracking.Application.Validations;
using FluentValidation;

namespace ESAM.GrowTracking.Application.Features.Auth.Login
{
    public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
    {
        public LoginCommandValidator()
        {
            RuleFor(lc => lc.Credential).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage(CommandValidationMessages.CredentialRequired)
                .MinimumLength(5).WithMessage(CommandValidationMessages.CredentialMinLength)
                .MaximumLength(50).WithMessage(CommandValidationMessages.CredentialMaxLength)
                .Must(CommandValidationRules.IsValidCredential).WithMessage((_, credential) => credential!.Contains('@')
                    ? CommandValidationMessages.CredentialValidatedEmail : CommandValidationMessages.CredentialValidatedUsername);
            RuleFor(lc => lc.Password).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage(CommandValidationMessages.PasswordRequired)
                .MinimumLength(5).WithMessage(CommandValidationMessages.PasswordMinLength)
                .MaximumLength(100).WithMessage(CommandValidationMessages.PasswordMaxLength);
            RuleFor(lc => lc.IsPersistent).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage(CommandValidationMessages.IsPersistentRequired);
            RuleFor(lc => lc.DeviceIdentifier).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage(CommandValidationMessages.DeviceIdentifierRequired)
                .MinimumLength(3).WithMessage(CommandValidationMessages.DeviceIdentifierMinLength)
                .MaximumLength(256).WithMessage(CommandValidationMessages.DeviceIdentifierMaxLength)
                .Must(CommandValidationRules.IsValidGuid).WithMessage(CommandValidationMessages.DeviceIdentifierInvalid);
            RuleFor(lc => lc.DeviceName).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage(CommandValidationMessages.DeviceNameRequired)
                .MinimumLength(2).WithMessage(CommandValidationMessages.DeviceNameMinLength)
                .MaximumLength(100).WithMessage(CommandValidationMessages.DeviceNameMaxLength)
                .Must(CommandValidationRules.HasNoControlChars).WithMessage(CommandValidationMessages.DeviceNameInvalid);
            RuleFor(lc => lc.ApiClientType).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage(CommandValidationMessages.ApiClientTypeRequired)
                .IsInEnum().WithMessage(CommandValidationMessages.ApiClientTypeInvalid);
        }
    }
}