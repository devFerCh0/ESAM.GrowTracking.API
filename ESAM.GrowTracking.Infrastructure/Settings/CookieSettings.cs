using ESAM.GrowTracking.Infrastructure.Http;

namespace ESAM.GrowTracking.Infrastructure.Settings
{
    public sealed class CookieSettings
    {
        public string CookieName { get; set; } = "rt";

        public bool UseHostPrefix { get; set; } = false;

        public bool AllowRefreshTokenHeader { get; set; } = true;

        public bool AlwaysSecure { get; set; } = false;

        public string XsrfCookieName { get; set; } = "XSRF-TOKEN";

        public int XsrfCookieExpiresMinutes { get; set; } = 30;

        public string? Domain { get; set; } = null;

        public SameSitePolicy SameSite { get; set; } = SameSitePolicy.Lax;

        public string Path { get; set; } = "/";

        public List<string> XsrfExemptPaths { get; set; } = ["/api/auth/login", "/api/auth/refresh"];

        public string EffectiveRefreshCookieName() => UseHostPrefix ? "__Host-" + CookieName : CookieName;

        public string EffectiveXsrfCookieName() => UseHostPrefix ? "__Host-" + XsrfCookieName : XsrfCookieName;

        public void Validate(bool isProduction)
        {
            if (string.IsNullOrWhiteSpace(CookieName))
                throw new InvalidOperationException($"{nameof(CookieName)} no puede estar vacío.");
            if (string.IsNullOrWhiteSpace(XsrfCookieName))
                throw new InvalidOperationException($"{nameof(XsrfCookieName)} no puede estar vacío.");
            if (string.Equals(EffectiveRefreshCookieName(), EffectiveXsrfCookieName(), StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException($"Los nombres efectivos de las cookies de refresh token ('{EffectiveRefreshCookieName()}') y " +
                    $"XSRF ('{EffectiveXsrfCookieName()}') no pueden ser iguales. " +
                    $"Asigne valores distintos a {nameof(CookieName)} y {nameof(XsrfCookieName)} para evitar colisiones.");
            if (XsrfCookieExpiresMinutes <= 0)
                throw new InvalidOperationException($"{nameof(XsrfCookieExpiresMinutes)} debe ser mayor a cero.");
            if (XsrfCookieExpiresMinutes > 1440)
                throw new InvalidOperationException($"{nameof(XsrfCookieExpiresMinutes)} no debe superar los 1440 minutos (24 horas) " +
                    "para limitar la ventana de exposición del token XSRF.");
            if (string.IsNullOrWhiteSpace(Path))
                throw new InvalidOperationException($"{nameof(Path)} no puede ser nulo o vacío.");
            if (!Path.StartsWith('/'))
                throw new InvalidOperationException($"{nameof(Path)} debe comenzar con '/' (RFC 6265 §5.2.4). Valor actual: '{Path}'.");
            if (!Enum.IsDefined(typeof(SameSitePolicy), SameSite))
                throw new InvalidOperationException($"{nameof(SameSite)} contiene un valor no definido '{SameSite}'. " +
                    $"Los valores válidos son: {string.Join(", ", Enum.GetNames<SameSitePolicy>())}.");
            if (isProduction && SameSite == SameSitePolicy.Unspecified)
                throw new InvalidOperationException($"{nameof(SameSite)} no puede ser '{nameof(SameSitePolicy.Unspecified)}' en producción. Use 'Lax', 'Strict' o 'None'.");
            if (UseHostPrefix)
            {
                if (!string.Equals(Path, "/", StringComparison.Ordinal))
                    throw new InvalidOperationException($"Cuando {nameof(UseHostPrefix)} es true, {nameof(Path)} debe ser '/'.");
                if (!string.IsNullOrWhiteSpace(Domain))
                    throw new InvalidOperationException($"Cuando {nameof(UseHostPrefix)} es true, {nameof(Domain)} debe ser nulo " +
                        "(las cookies con prefijo __Host- no permiten especificar dominio).");
                if (!AlwaysSecure)
                    throw new InvalidOperationException($"Cuando {nameof(UseHostPrefix)} es true, {nameof(AlwaysSecure)} debe ser true (requerido por el prefijo __Host-).");
            }
            if (SameSite == SameSitePolicy.None && isProduction && !AlwaysSecure && !UseHostPrefix)
                throw new InvalidOperationException("CookieSettings: SameSite=None requires Secure=true in non-development environments. " +
                    "Set AlwaysSecure=true or UseHostPrefix=true. Browsers reject SameSite=None cookies without the Secure attribute.");
            if (XsrfExemptPaths.Any(string.IsNullOrWhiteSpace))
                throw new InvalidOperationException($"Uno o más valores en {nameof(XsrfExemptPaths)} son nulos o están vacíos.");
            if (XsrfExemptPaths.Any(p => !p.StartsWith('/')))
                throw new InvalidOperationException($"Todos los valores en {nameof(XsrfExemptPaths)} deben comenzar con '/'.");
        }
    }
}