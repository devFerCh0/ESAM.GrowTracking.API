using ESAM.GrowTracking.API.Serialization;
using ESAM.GrowTracking.API.Validations;
using ESAM.GrowTracking.Domain.Enums;
using FluentValidation;

namespace ESAM.GrowTracking.API.Controllers.Auth.Login
{
    public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
    {
        public LoginRequestValidator()
        {
            RuleFor(lc => lc.Credential).Cascade(CascadeMode.Stop)
                .NotNull().WithMessage(RequestValidationMessages.CredentialRequired);
            RuleFor(lc => lc.Password).Cascade(CascadeMode.Stop)
                .NotNull().WithMessage(RequestValidationMessages.PasswordRequired);
            RuleFor(lc => lc.IsPersistent)
                .NotNull().WithMessage(RequestValidationMessages.IsPersistentRequired);
            RuleFor(lc => lc.DeviceIdentifier).Cascade(CascadeMode.Stop)
                .NotNull().WithMessage(RequestValidationMessages.DeviceIdentifierRequired);
            RuleFor(lc => lc.DeviceName).Cascade(CascadeMode.Stop)
                .NotNull().WithMessage(RequestValidationMessages.DeviceNameRequired);
            RuleFor(lc => lc.ApiClientType).Cascade(CascadeMode.Stop)
                .NotNull().WithMessage(RequestValidationMessages.ApiClientTypeRequired)
                .Must(EnumHelper.IsValidFromString<ApiClientType>).WithMessage(RequestValidationMessages.ApiClientTypeInvalid);
        }
    }
}