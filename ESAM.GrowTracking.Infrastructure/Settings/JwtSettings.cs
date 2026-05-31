namespace ESAM.GrowTracking.Infrastructure.Settings
{
    public sealed class JwtSettings
    {
        private const int MinSecretKeyLength = 32;
        private const int MinDistinctSecretKeyChars = 8;

        public string Issuer { get; set; } = string.Empty;

        public string Audience { get; set; } = string.Empty;

        public string SecretKey { get; set; } = string.Empty;

        public void Validate(bool isProduction)
        {
            if (string.IsNullOrWhiteSpace(Issuer))
                throw new InvalidOperationException($"{nameof(Issuer)} debe proporcionarse.");
            if (!Uri.TryCreate(Issuer, UriKind.Absolute, out _))
                throw new InvalidOperationException($"{nameof(Issuer)} debe ser una URI absoluta válida (RFC 7519 §4.1.1). Ejemplo: 'https://auth.midominio.com'.");
            if (string.IsNullOrWhiteSpace(Audience))
                throw new InvalidOperationException($"{nameof(Audience)} debe proporcionarse.");
            if (!Uri.TryCreate(Audience, UriKind.Absolute, out _))
                throw new InvalidOperationException($"{nameof(Audience)} debe ser una URI absoluta válida (RFC 7519 §4.1.3). Ejemplo: 'https://api.midominio.com'.");
            if (string.IsNullOrWhiteSpace(SecretKey))
                throw new InvalidOperationException($"{nameof(SecretKey)} debe proporcionarse.");
            if (SecretKey.Length < MinSecretKeyLength)
                throw new InvalidOperationException($"{nameof(SecretKey)} debe tener al menos {MinSecretKeyLength * 8} bits ({MinSecretKeyLength} bytes) para HMAC-SHA256.");
            if (SecretKey.Distinct().Count() < MinDistinctSecretKeyChars)
                throw new InvalidOperationException($"{nameof(SecretKey)} tiene entropía insuficiente (menos de {MinDistinctSecretKeyChars} caracteres distintos). " +
                    $"Utilice una clave criptográficamente aleatoria de al menos {MinSecretKeyLength * 8} bits.");
            if (isProduction)
            {
                if (!Issuer.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException($"{nameof(Issuer)} debe usar el esquema HTTPS en producción. Valor actual: '{Issuer}'.");
                if (Issuer.Contains("localhost", StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException($"{nameof(Issuer)} no puede referenciar 'localhost' en producción.");
                if (Audience.Contains("localhost", StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException($"{nameof(Audience)} no puede referenciar 'localhost' en producción.");
                var lowerKey = SecretKey.ToLowerInvariant();
                var weakPatterns = new[] { "dev", "test", "secret", "changeme", "password", "demo", "sample" };
                if (weakPatterns.Any(lowerKey.Contains))
                    throw new InvalidOperationException($"{nameof(SecretKey)} parece contener un valor de desarrollo o prueba. " +
                        "Use una clave generada criptográficamente en producción.");
            }
        }
    }
}