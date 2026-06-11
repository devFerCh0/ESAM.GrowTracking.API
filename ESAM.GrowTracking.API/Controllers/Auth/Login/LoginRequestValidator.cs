using ESAM.GrowTracking.Domain.Enums;
using FluentValidation;

namespace ESAM.GrowTracking.API.Controllers.Auth.Login
{
    public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
    {
        public LoginRequestValidator()
        {
            RuleFor(lc => lc.Credential).Cascade(CascadeMode.Stop)
                .NotNull().WithMessage(ValidationMessages.CredentialRequired);

            RuleFor(lc => lc.Password).Cascade(CascadeMode.Stop)
                .NotNull().WithMessage(ValidationMessages.PasswordRequired);

            RuleFor(lc => lc.IsPersistent)
                .NotNull().WithMessage(ValidationMessages.IsPersistentRequired);

            RuleFor(lc => lc.DeviceIdentifier).Cascade(CascadeMode.Stop)
                .NotNull().WithMessage(ValidationMessages.DeviceIdentifierRequired);

            RuleFor(lc => lc.DeviceName).Cascade(CascadeMode.Stop)
                .NotNull().WithMessage(ValidationMessages.DeviceNameRequired);

            RuleFor(lc => lc.ApiClientType).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage(ValidationMessages.ApiClientTypeRequired)
                .Must(ValidationRules.BeAValidEnum<ApiClientType>).WithMessage(ValidationMessages.ApiClientTypeInvalid);


            //RuleFor(x => x.Credential)
            //    .NotNull().WithMessage("La credencial es obligatoria.");

            //RuleFor(x => x.Password)
            //    .NotNull().WithMessage("La contraseña es obligatoria.");

            //RuleFor(x => x.IsPersistent)
            //    .NotNull().WithMessage("El campo IsPersistent es obligatorio.");

            //RuleFor(x => x.DeviceIdentifier)
            //    .NotNull().WithMessage("El identificador del dispositivo es obligatorio.");

            //RuleFor(x => x.DeviceName)
            //    .NotNull().WithMessage("El nombre del dispositivo es obligatorio.");

            RuleFor(x => x.ApiClientType)
                .NotNull().WithMessage("El tipo de cliente es obligatorio.");
        }
    }
}