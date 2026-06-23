using ESAM.GrowTracking.API.Validations;
using ESAM.GrowTracking.Domain.Enums;
using ESAM.GrowTracking.Infrastructure.Utilities;
using FluentValidation;

namespace ESAM.GrowTracking.API.Controllers.Auth.Login
{
    public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
    {
        public LoginRequestValidator()
        {
            RuleFor(lr => lr.Credential).Cascade(CascadeMode.Stop)
                .NotNull().WithMessage(RequestValidationMessages.CredentialRequired);
            RuleFor(lr => lr.Password).Cascade(CascadeMode.Stop)
                .NotNull().WithMessage(RequestValidationMessages.PasswordRequired);
            RuleFor(lr => lr.IsPersistent)
                .NotNull().WithMessage(RequestValidationMessages.IsPersistentRequired);
            RuleFor(lr => lr.DeviceIdentifier).Cascade(CascadeMode.Stop)
                .NotNull().WithMessage(RequestValidationMessages.DeviceIdentifierRequired);
            RuleFor(lr => lr.DeviceName).Cascade(CascadeMode.Stop)
                .NotNull().WithMessage(RequestValidationMessages.DeviceNameRequired);
            RuleFor(lr => lr.ApiClientType).Cascade(CascadeMode.Stop)
                .NotNull().WithMessage(RequestValidationMessages.ApiClientTypeRequired)
                .Must(EnumHelper.IsValidFromString<ApiClientType>).WithMessage(RequestValidationMessages.ApiClientTypeInvalid);
        }
    }
}