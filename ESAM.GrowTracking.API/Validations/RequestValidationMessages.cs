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
    }
}