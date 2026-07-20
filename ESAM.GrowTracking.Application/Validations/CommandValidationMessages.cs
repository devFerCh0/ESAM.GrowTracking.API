namespace ESAM.GrowTracking.Application.Validations
{
    public static class CommandValidationMessages
    {
        public const string CredentialRequired = "El nombre de usuario o correo electrónico es obligatorio.";
        public const string CredentialMaxLength = "El nombre de usuario o correo electrónico no debe exceder de 50 caracteres.";
        public const string CredentialMinLength = "El nombre de usuario o correo electrónico debe tener al menos 5 caracteres.";
        public const string CredentialValidatedEmail = "El correo electrónico no es válido.";
        public const string CredentialValidatedUsername = "El nombre de usuario contiene caracteres no permitidos.";
        public const string PasswordRequired = "La contraseña es obligatoria.";
        public const string PasswordMinLength = "La contraseña debe tener al menos 5 caracteres.";
        public const string PasswordMaxLength = "La contraseña no debe exceder de 100 caracteres.";
        public const string IsPersistentRequired = "La opción 'Recuérdame' es obligatoria.";
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
        public const string RefreshTokenRequired = "El refresh token es obligatorio.";
        public const string RefreshTokenMinLength = "El refresh token debe de tener al menos 32 caracteres.";
        public const string RefreshTokenMaxLength = "El refresh token no puede exceder los 256 caracteres.";
        public const string RefreshTokenInvalid = "El refresh token no es válido.";
        public const string AccessTokenRequired = "El access token no es válido.";
        public const string AccessTokenMinLength = "El access token debe de tener al menos 256 caracteres.";
        public const string AccessTokenMaxLength = "El access token no puede exceder los 1024 caracteres.";
        public const string UserIdRequired = "El usuario es obligatorio.";
        public const string UserIdInvalid = "El usuario no es válido.";
        public const string UserSessionIdRequired = "La sesión de usuario es obligatoria.";
        public const string UserSessionIdInvalid = "La sesión de usuario no es válida.";
        public const string CurrentPasswordRequired = "La contraseña actual es obligatoria.";
        public const string CurrentPasswordMinLength = "La contraseña actual debe tener al menos 5 caracteres.";
        public const string CurrentPasswordMaxLength = "La contraseña actual no puede exceder los 100 caracteres.";
        public const string NewPasswordRequired = "La nueva contraseña es obligatoria.";
        public const string NewPasswordMinLength = "La nueva contraseña debe tener al menos 5 caracteres.";
        public const string NewPasswordMaxLength = "La nueva contraseña no puede exceder los 100 caracteres.";
        public const string NewPasswordSameAsCurrent = "La nueva contraseña no puede ser igual a la contraseña actual.";
        public const string ConfirmNewPasswordRequired = "La confirmación de la nueva contraseña es obligatoria.";
        public const string ConfirmNewPasswordMismatch = "La confirmación no coincide con la nueva contraseña.";
        public const string UserDeviceIdRequired = "El dispositivo de usuario es obligatorio.";
        public const string UserDeviceIdInvalid = "El dispositivo de usuario no es válido.";
        public const string LockoutEndAtRequired = "La fecha de bloqueo es obligatoria.";
        public const string LockoutEndAtInvalid = "La fecha de bloqueo debe ser posterior a la fecha y hora actual.";
        public const string SearchTermMinLength = "El término de búsqueda debe tener al menos 2 caracteres.";
        public const string SearchTermMaxLength = "El término de búsqueda no puede exceder los 100 caracteres.";
        public const string SearchTermInvalid = "El término de búsqueda contiene caracteres no permitidos.";
        public const string SortByInvalid = "El campo de ordenamiento especificado no es válido.";
        public const string SortDirectionInvalid = "La dirección de ordenamiento especificada no es válida.";
        public const string PageNumberInvalid = "El número de página debe ser mayor a cero.";
        public const string PageSizeInvalid = "El tamaño de página debe estar entre 1 y 100.";
    }
}