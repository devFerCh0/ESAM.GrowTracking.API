namespace ESAM.GrowTracking.Persistence.Settings
{
    public sealed class ConnectionStringSettings
    {
        public string DefaultConnection { get; set; } = string.Empty;

        public void Validate(bool isProduction)
        {
            if (string.IsNullOrWhiteSpace(DefaultConnection))
                throw new InvalidOperationException($"La cadena de conexión {nameof(DefaultConnection)} no está configurada.");
            if (DefaultConnection.Length < 20)
                throw new InvalidOperationException($"La cadena de conexión {nameof(DefaultConnection)} parece demasiado corta para ser válida.");
            if (!DefaultConnection.Contains("Server=", StringComparison.OrdinalIgnoreCase) && !DefaultConnection.Contains("Data Source=", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException($"La cadena de conexión {nameof(DefaultConnection)} debe especificar el servidor mediante 'Server=' o 'Data Source='.");
            if (!DefaultConnection.Contains("Database=", StringComparison.OrdinalIgnoreCase) && !DefaultConnection.Contains("Initial Catalog=", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException($"La cadena de conexión {nameof(DefaultConnection)} debe especificar la " +
                    "base de datos mediante 'Database=' o 'Initial Catalog='.");
            if (isProduction && DefaultConnection.Contains("TrustServerCertificate=True", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("'TrustServerCertificate=True' detectado en producción. Use certificados TLS válidos.");
            if (isProduction && DefaultConnection.Contains("Encrypt=False", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("'Encrypt=False' detectado en producción. Todas las conexiones deben estar cifradas en producción.");
        }
    }
}