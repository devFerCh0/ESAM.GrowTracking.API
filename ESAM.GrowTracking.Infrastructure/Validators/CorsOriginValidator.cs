using ESAM.GrowTracking.Infrastructure.Abstractions.Validators;
using ESAM.GrowTracking.Infrastructure.Cors;
using ESAM.GrowTracking.Infrastructure.Settings;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;

namespace ESAM.GrowTracking.Infrastructure.Validators
{
    public sealed class CorsOriginValidator : ICorsOriginValidator
    {
        private readonly HashSet<string> _allowedOrigins;
        private readonly CompiledOriginWildcard[] _compiledWildcards;
        private readonly Regex[] _compiledRegexes;
        private readonly bool _allowHttpOnLocalhost;
        private readonly IReadOnlyList<int> _localhostPorts;
        private readonly bool _isProduction;
        private readonly ILogger<CorsOriginValidator> _logger;

        public CorsOriginValidator(IOptions<CorsSettings> corsSettingsOptions, IHostEnvironment env, ILogger<CorsOriginValidator> logger)
        {
            ArgumentNullException.ThrowIfNull(corsSettingsOptions);
            ArgumentNullException.ThrowIfNull(env);
            ArgumentNullException.ThrowIfNull(logger);
            var corsSettings = corsSettingsOptions.Value ?? throw new ArgumentNullException(nameof(corsSettingsOptions));
            _isProduction = env.IsProduction();
            _logger = logger;
            _allowHttpOnLocalhost = corsSettings.AllowHttpOnLocalhost;
            _localhostPorts = (corsSettings.LocalhostPorts ?? []).AsReadOnly();
            _allowedOrigins = BuildAllowedOriginsSet(corsSettings);
            var enforceHttps = _isProduction && corsSettings.EnforceStrictOriginsInProduction;
            _compiledWildcards = BuildCompiledWildcards(corsSettings.AllowedOriginWildcards, enforceHttps);
            _compiledRegexes = BuildCompiledRegexes(corsSettings.AllowedOriginRegex, logger);
            if (enforceHttps)
            {
                ValidateProductionOrigins(_allowedOrigins);
                ValidateProductionWildcards(_compiledWildcards);
            }
        }

        public bool IsOriginAllowed(string origin)
        {
            if (string.IsNullOrWhiteSpace(origin))
                return false;
            if (_allowedOrigins.Contains(origin))
                return true;
            Uri.TryCreate(origin, UriKind.Absolute, out var originUri);
            if (originUri is not null)
            {
                if (!_isProduction && _allowHttpOnLocalhost && IsLocalhost(originUri, _localhostPorts))
                    return true;
                foreach (var wc in _compiledWildcards)
                    if (MatchWildcard(originUri, wc))
                        return true;
            }
            foreach (var rx in _compiledRegexes)
            {
                try
                {
                    if (rx.IsMatch(origin))
                        return true;
                }
                catch (RegexMatchTimeoutException te)
                {
                    _logger.LogWarning(te, "CORS: el regex '{Pattern}' excedió el tiempo de espera para el origen '{Origin}'.", rx, origin);
                }
            }
            if (_isProduction)
                _logger.LogWarning("CORS: origen denegado '{Origin}'. No coincide con ninguna regla de AllowedOrigins. Revise CorsSettings si este origen es legítimo.", origin);
            return false;
        }

        private static HashSet<string> BuildAllowedOriginsSet(CorsSettings corsSettings)
        {
            if (corsSettings.AllowedOrigins is null or { Count: 0 })
                return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            return new HashSet<string>(corsSettings.AllowedOrigins.Where(o => !string.IsNullOrWhiteSpace(o)).Select(o => o.Trim()), StringComparer.OrdinalIgnoreCase);
        }

        private static CompiledOriginWildcard[] BuildCompiledWildcards(List<string>? wildcards, bool enforceHttps)
        {
            if (wildcards is null or { Count: 0 })
                return [];
            var result = new List<CompiledOriginWildcard>(wildcards.Count);
            foreach (var raw in wildcards)
            {
                var w = raw.Trim();
                if (string.IsNullOrEmpty(w))
                    continue;
                if (enforceHttps && !w.Contains("://", StringComparison.Ordinal))
                    w = $"https://{w}";
                string? scheme = null;
                if (w.Contains("://", StringComparison.Ordinal))
                {
                    var idx = w.IndexOf("://", StringComparison.Ordinal);
                    scheme = w[..idx];
                    w = w[(idx + 3)..];
                }
                w = w.TrimEnd('/');
                var hostPart = w;
                int? port = null;
                if (hostPart.Contains(':', StringComparison.Ordinal))
                {
                    var parts = hostPart.Split(':', 2);
                    hostPart = parts[0];
                    if (int.TryParse(parts[1], out var p))
                        port = p;
                }
                var escaped = Regex.Escape(hostPart).Replace("\\*", "[^.]*", StringComparison.Ordinal);
                var hostRegex = new Regex($"^{escaped}$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled, TimeSpan.FromMilliseconds(100));
                result.Add(new CompiledOriginWildcard { Scheme = scheme, HostRegex = hostRegex, Port = port });
            }
            return [.. result];
        }

        private static Regex[] BuildCompiledRegexes(List<string>? patterns, ILogger logger)
        {
            if (patterns is null or { Count: 0 })
                return [];
            var result = new List<Regex>(patterns.Count);
            foreach (var raw in patterns)
            {
                var pattern = raw.Trim();
                if (string.IsNullOrEmpty(pattern))
                    continue;
                try
                {
                    result.Add(new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled, TimeSpan.FromMilliseconds(100)));
                }
                catch (ArgumentException ex)
                {
                    logger.LogWarning(ex, "CORS: regex inválido '{Pattern}' en AllowedOriginRegex. Será ignorado.", pattern);
                }
            }
            return [.. result];
        }

        private static void ValidateProductionOrigins(HashSet<string> allowedOrigins)
        {
            if (allowedOrigins.Any(o => o == "*" || o.Equals("null", StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException("CORS: wildcard origins ('*' / 'null') no están permitidos en producción.");
            var insecure = allowedOrigins.Where(o =>
            {
                if (Uri.TryCreate(o, UriKind.Absolute, out var u))
                    return !string.Equals(u.Scheme, "https", StringComparison.OrdinalIgnoreCase);
                return true;
            }).ToList();
            if (insecure.Count != 0)
                throw new InvalidOperationException($"CORS: orígenes inseguros (no-https) encontrados para producción: {string.Join(", ", insecure)}. " +
                    "Asegúrese de incluir el esquema 'https://' en AllowedOrigins.");
        }

        private static void ValidateProductionWildcards(CompiledOriginWildcard[] compiledWildcards)
        {
            var insecure = compiledWildcards.Where(wc => !string.Equals(wc.Scheme, "https", StringComparison.OrdinalIgnoreCase)).Select(wc => wc.HostRegex.ToString()).ToList();
            if (insecure.Count != 0)
                throw new InvalidOperationException($"CORS: wildcards sin esquema 'https://' encontrados para producción: {string.Join(", ", insecure)}. " +
                    "Añadir el esquema en AllowedOriginWildcards (p.ej. 'https://*.dominio.com').");
        }

        private static bool IsLocalhost(Uri originUri, IReadOnlyList<int> localhostPorts) => (originUri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
            || originUri.Host.Equals("127.0.0.1", StringComparison.Ordinal)) && localhostPorts.Contains(originUri.Port);

        private static bool MatchWildcard(Uri originUri, CompiledOriginWildcard wc)
        {
            if (!string.IsNullOrEmpty(wc.Scheme) && !string.Equals(wc.Scheme, originUri.Scheme, StringComparison.OrdinalIgnoreCase))
                return false;
            try
            {
                if (!wc.HostRegex.IsMatch(originUri.Host))
                    return false;
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
            if (wc.Port.HasValue && originUri.Port != wc.Port.Value)
                return false;
            return true;
        }
    }
}