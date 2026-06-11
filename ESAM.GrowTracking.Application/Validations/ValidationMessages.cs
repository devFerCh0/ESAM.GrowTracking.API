namespace ESAM.GrowTracking.Application.Validations
{
    internal static class ValidationMessages
    {
        public const string CredentialRequired = "El nombre de usuario o correo electrónico es obligatorio.";
        public const string CredentialMaxLength = "El nombre de usuario o correo electrónico no debe exceder de 50 caracteres.";
        public const string CredentialMinLength = "El nombre de usuario o correo electrónico debe tener al menos 5 caracteres.";
        public const string CredentialValidatedEmail = "El correo electrónico no es válido.";
        public const string CredentialValidatedUsername = "El nombre de usuario contiene caracteres no permitidos.";
        public const string PasswordRequired = "La contraseña es obligatoria.";
        public const string PasswordMinLength = "La contraseña debe tener al menos 5 caracteres.";
        public const string PasswordMaxLength = "La contraseña no debe exceder de 100 caracteres.";
        public const string IsPersistentRequired = "El campo recordame es obligatorio.";
        public const string DeviceIdentifierRequired = "El identificador del dispositivo es obligatorio.";
        public const string DeviceIdentifierMinLength = "El identificador del dispositivo debe de tener al menos 3 caracteres.";
        public const string DeviceIdentifierMaxLength = "El identificador del dispositivo no puede exceder los 256 caracteres.";
        public const string DeviceIdentifierInvalid = "El identificador del dispositivo no es válido. Use un GUID o un identificador alfanumérico seguro (3–256 caracteres).";
        public const string DeviceNameRequired = "El nombre del dispositivo es obligatorio.";
        public const string DeviceNameMinLength = "El nombre del dispositivo debe tener al menos 2 caracteres.";
        public const string DeviceNameMaxLength = "El nombre del dispositivo no puede exceder de 100 caracteres.";
        public const string DeviceNameInvalid = "El nombre del dispositivo contiene caracteres no permitidos.";
        public const string ApiClientTypeRequired = "El tipo de cliente es obligatorio.";
        public const string ApiClientTypeInvalid = "El tipo de cliente no es válido.";
        public const string WorkProfileIdRequired = "El perfil de trabajo es obligatorio.";
        public const string WorkProfileIdInvalid = "El perfil de trabajo no es válido.";
        public const string RoleIdRequired = "El rol es obligatorio.";
        public const string RoleIdInvalid = "El rol no es válido.";
        public const string CampusIdRequired = "La sede es obligatoria.";
        public const string CampusIdInvalid = "la sede no es válida.";
        public const string RefreshTokenMinLength = "El refresh token debe de tener al menos 3 caracteres.";
        public const string RefreshTokenMaxLength = "El refresh token no puede exceder los 256 caracteres.";
    }
}