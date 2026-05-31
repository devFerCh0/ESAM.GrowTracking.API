namespace ESAM.GrowTracking.API.Settings
{
    internal sealed class DataProtectionSettings
    {
        public bool? IsDistributed { get; set; }

        public string? KeysPath { get; set; }

        public void Validate(bool isProduction)
        {
            if (!IsDistributed.HasValue)
                throw new InvalidOperationException($"El campo {nameof(IsDistributed)} es obligatorio.");
            if (string.IsNullOrWhiteSpace(KeysPath))
                throw new InvalidOperationException($"El campo {nameof(KeysPath)} es obligatorio.");
            if (KeysPath.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
                throw new InvalidOperationException($"El campo {nameof(KeysPath)} contiene caracteres no válidos.");
            if (!Path.IsPathRooted(KeysPath))
                throw new InvalidOperationException($"El campo {nameof(KeysPath)} debe ser una ruta absoluta.");
            if (isProduction)
            {
                var normalizedPath = KeysPath.Trim();
                if (normalizedPath.Contains("/tmp", StringComparison.OrdinalIgnoreCase) || normalizedPath.Contains(@"\temp", StringComparison.OrdinalIgnoreCase) ||
                    normalizedPath.Contains(@"AppData\Local", StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException($"El campo {nameof(KeysPath)} no puede apuntar a una ruta temporal o no persistente en producción.");
            }
        }
    }
}