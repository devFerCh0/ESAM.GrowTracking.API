namespace ESAM.GrowTracking.API.Validations
{
    public static class RequestValidationMessages
    {
        public const string CredentialRequired = "La credencial es obligatoria.";
        public const string PasswordRequired = "La contraseña es obligatoria.";
        public const string IsPersistentRequired = "La opción 'Recuérdame' es obligatoria.";
        public const string DeviceIdentifierRequired = "El identificador del dispositivo es obligatorio.";
        public const string DeviceNameRequired = "El nombre del dispositivo es obligatorio.";
        public const string ApiClientTypeRequired = "El tipo de cliente es obligatorio.";
        public const string ApiClientTypeInvalid = "El tipo de cliente no es válido. Valores permitidos: Web, Mobile o Desktop.";
        public const string WorkProfileIdRequired = "El perfil de trabajo es obligatorio.";
        public const string RoleIdRequired = "El rol es obligatorio.";
        public const string CampusIdRequired = "La sede es obligatoria.";
        public const string RefreshTokenRequired = "El refresh token es obligatorio.";
        public const string AccessTokenRequired = "El access token no es válido.";
        public const string UserIdRequired = "El usuario es obligatorio.";
        public const string UserSessionIdRequired = "La sesión de usuario es obligatoria.";
        public const string CurrentPasswordRequired = "La contraseña actual es obligatoria.";
        public const string NewPasswordRequired = "La nueva contraseña es obligatoria.";
        public const string ConfirmNewPasswordRequired = "La confirmación de la nueva contraseña es obligatoria.";
    }
}