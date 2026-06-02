namespace ESAM.GrowTracking.Infrastructure.Settings
{
    public sealed class ClientInfoSettings
    {
        public List<string> IpHeaderKeys { get; set; } = [];

        public void Validate()
        {
            if (IpHeaderKeys == null || IpHeaderKeys.Count == 0)
                throw new InvalidOperationException($"El campo {nameof(IpHeaderKeys)} no puede estar vacío. " + 
                    "Se requiere al menos un encabezado para identificar la IP del cliente.");
            if (IpHeaderKeys.Any(string.IsNullOrWhiteSpace))
                throw new InvalidOperationException($"Uno o más valores en {nameof(IpHeaderKeys)} son nulos o están vacíos.");
            var duplicates = IpHeaderKeys.GroupBy(k => k.Trim(), StringComparer.OrdinalIgnoreCase).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
            if (duplicates.Count > 0)
                throw new InvalidOperationException($"Existen encabezados duplicados en {nameof(IpHeaderKeys)}: {string.Join(", ", duplicates)}. Cada encabezado debe ser único.");
        }
    }
}