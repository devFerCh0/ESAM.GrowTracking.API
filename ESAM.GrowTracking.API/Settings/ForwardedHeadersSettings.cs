using System.Net;
using System.Net.Sockets;

namespace ESAM.GrowTracking.API.Settings
{
    internal sealed class ForwardedHeadersSettings
    {
        public List<string> KnownNetworks { get; set; } = [];

        public List<string> KnownProxies { get; set; } = [];

        public void Validate()
        {
            foreach (var cidr in KnownNetworks ?? [])
            {
                if (string.IsNullOrWhiteSpace(cidr))
                    throw new InvalidOperationException($"Un valor en {nameof(KnownNetworks)} es nulo o está vacío.");
                var parts = cidr.Split('/', 2);
                if (parts.Length != 2 || !IPAddress.TryParse(parts[0], out var parsedIp) || !int.TryParse(parts[1], out var prefix))
                    throw new InvalidOperationException($"Formato CIDR inválido '{cidr}' en {nameof(KnownNetworks)}. Use notación 'x.x.x.x/n' para IPv4 o 'xxxx::/n' para IPv6.");
                var maxPrefix = parsedIp.AddressFamily == AddressFamily.InterNetworkV6 ? 128 : 32;
                if (prefix < 0 || prefix > maxPrefix)
                    throw new InvalidOperationException($"Prefijo CIDR fuera de rango en '{cidr}' en {nameof(KnownNetworks)}. Valor válido: 0–{maxPrefix}.");
                if (prefix == 0)
                    throw new InvalidOperationException($"El prefijo '/0' en '{cidr}' en {nameof(KnownNetworks)} es excesivamente permisivo: representaría todas las " +
                        "IPs posibles como proxies confiables, lo que anularía la protección de cabeceras reenviadas. Use rangos específicos.");
            }
            foreach (var proxy in KnownProxies ?? [])
            {
                if (string.IsNullOrWhiteSpace(proxy))
                    throw new InvalidOperationException($"Un valor en {nameof(KnownProxies)} es nulo o está vacío.");
                if (!IPAddress.TryParse(proxy, out _))
                    throw new InvalidOperationException($"IP de proxy inválida '{proxy}' en {nameof(KnownProxies)}. Proporcione una dirección IPv4 o IPv6 válida.");
            }
            var duplicateNetworks = (KnownNetworks ?? []).GroupBy(n => n?.Trim() ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1 && !string.IsNullOrWhiteSpace(g.Key)).Select(g => g.Key).ToList();
            if (duplicateNetworks.Count > 0)
                throw new InvalidOperationException($"Se encontraron redes duplicadas en {nameof(KnownNetworks)}: {string.Join(", ", duplicateNetworks)}. " +
                    "Elimine las entradas repetidas.");
            var duplicateProxies = (KnownProxies ?? []).GroupBy(p => p?.Trim() ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1 && !string.IsNullOrWhiteSpace(g.Key)).Select(g => g.Key).ToList();
            if (duplicateProxies.Count > 0)
                throw new InvalidOperationException($"Se encontraron proxies duplicados en {nameof(KnownProxies)}: {string.Join(", ", duplicateProxies)}. " +
                    "Elimine las entradas repetidas.");
        }
    }
}