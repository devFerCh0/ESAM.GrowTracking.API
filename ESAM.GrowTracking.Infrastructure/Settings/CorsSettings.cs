namespace ESAM.GrowTracking.Infrastructure.Settings
{
    public sealed class CorsSettings
    {
        public string PolicyName { get; set; } = "CorsPolicy";

        public List<string> AllowedOrigins { get; set; } = [];

        public List<string> AllowedOriginWildcards { get; set; } = [];

        public List<string> AllowedOriginRegex { get; set; } = [];

        public List<string> AllowedHeaders { get; set; } = ["Content-Type", "Authorization", "X-Requested-With", "X-XSRF-TOKEN", "X-Refresh-Token"];

        public List<string> AllowedMethods { get; set; } = ["GET", "HEAD", "POST", "PUT", "DELETE", "PATCH", "OPTIONS"];

        public List<string> ExposeHeaders { get; set; } = ["X-XSRF-TOKEN"];

        public bool AllowCredentials { get; set; } = true;

        public int PreflightMaxAgeSeconds { get; set; } = 600;

        public bool AllowHttpOnLocalhost { get; set; } = false;

        public List<int> LocalhostPorts { get; set; } = [3000, 4200, 8080];

        public bool EnforceStrictOriginsInProduction { get; set; } = true;

        public void Validate(bool isProduction)
        {
            if (string.IsNullOrWhiteSpace(PolicyName))
                throw new InvalidOperationException($"{nameof(PolicyName)} no puede estar vacío.");
            if (AllowedOrigins.Count == 0 && AllowedOriginWildcards.Count == 0 && AllowedOriginRegex.Count == 0)
                throw new InvalidOperationException("Debe definirse al menos un origen permitido " +
                    $"({nameof(AllowedOrigins)}, {nameof(AllowedOriginWildcards)} o {nameof(AllowedOriginRegex)}).");
            if (AllowCredentials && AllowedOrigins.Contains("*"))
                throw new InvalidOperationException($"No se puede usar '*' en {nameof(AllowedOrigins)} cuando {nameof(AllowCredentials)} es true.");
            if (AllowedOrigins.Any(string.IsNullOrWhiteSpace))
                throw new InvalidOperationException($"Uno o más valores en {nameof(AllowedOrigins)} son nulos o están vacíos.");
            if (AllowedOriginWildcards.Any(string.IsNullOrWhiteSpace))
                throw new InvalidOperationException($"Uno o más valores en {nameof(AllowedOriginWildcards)} son nulos o están vacíos.");
            if (AllowedOriginRegex.Any(string.IsNullOrWhiteSpace))
                throw new InvalidOperationException($"Uno o más valores en {nameof(AllowedOriginRegex)} son nulos o están vacíos.");
            if (AllowedHeaders is { Count: > 0 } && AllowedHeaders.Any(string.IsNullOrWhiteSpace))
                throw new InvalidOperationException($"Uno o más valores en {nameof(AllowedHeaders)} son nulos o están vacíos.");
            if (AllowedMethods is { Count: > 0 } && AllowedMethods.Any(string.IsNullOrWhiteSpace))
                throw new InvalidOperationException($"Uno o más valores en {nameof(AllowedMethods)} son nulos o están vacíos.");
            var duplicateOrigins = AllowedOrigins.GroupBy(o => o?.Trim() ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1 && !string.IsNullOrWhiteSpace(g.Key)).Select(g => g.Key).ToList();
            if (duplicateOrigins.Count > 0)
                throw new InvalidOperationException($"Se encontraron orígenes duplicados en {nameof(AllowedOrigins)}: {string.Join(", ", duplicateOrigins)}. " +
                    "Elimine las entradas repetidas.");
            foreach (var origin in AllowedOrigins)
            {
                var trimmed = origin?.Trim();
                if (string.IsNullOrWhiteSpace(trimmed) || trimmed == "*" || trimmed.Equals("null", StringComparison.OrdinalIgnoreCase))
                    continue;
                if (!Uri.TryCreate(trimmed, UriKind.Absolute, out var originUri) || !string.IsNullOrEmpty(originUri.Query) || !string.IsNullOrEmpty(originUri.Fragment) ||
                    originUri.AbsolutePath.TrimStart('/').Length > 0)
                    throw new InvalidOperationException($"El origen '{origin}' en {nameof(AllowedOrigins)} no tiene formato válido. " +
                        "Utilice 'scheme://host' o 'scheme://host:port' sin ruta, consulta ni fragmento.");
            }
            if (PreflightMaxAgeSeconds < 0)
                throw new InvalidOperationException($"{nameof(PreflightMaxAgeSeconds)} debe ser un valor positivo.");
            if (PreflightMaxAgeSeconds > 86400)
                throw new InvalidOperationException($"{nameof(PreflightMaxAgeSeconds)} no puede exceder 86 400 segundos (24 horas). " +
                    "Los navegadores aplican este límite máximo internamente y valores superiores son ignorados.");
            if (LocalhostPorts.Any(p => p <= 0 || p > 65535))
                throw new InvalidOperationException($"Uno o más puertos en {nameof(LocalhostPorts)} no son válidos.");
            if (isProduction)
            {
                if (AllowedOrigins.Any(o => o.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && !o.Contains("localhost", StringComparison.OrdinalIgnoreCase)))
                    throw new InvalidOperationException("No se permiten orígenes HTTP (no seguros) en producción.");
                if (AllowedOrigins.Contains("*") && EnforceStrictOriginsInProduction)
                    throw new InvalidOperationException(
                        $"El uso de '*' en {nameof(AllowedOrigins)} está prohibido en producción por {nameof(EnforceStrictOriginsInProduction)}.");
            }
        }
    }
}