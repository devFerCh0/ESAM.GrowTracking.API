namespace ESAM.GrowTracking.Persistence.Settings
{
    public sealed class DatabaseSettings
    {
        public bool? AutoMigrate { get; set; }

        public void Validate(bool isProduction)
        {
            if (!AutoMigrate.HasValue)
                throw new InvalidOperationException($"El campo {nameof(AutoMigrate)} es obligatorio.");
            if (isProduction && AutoMigrate == true)
                throw new InvalidOperationException($"'{nameof(AutoMigrate)} = true' está bloqueado en producción.");
        }
    }
}