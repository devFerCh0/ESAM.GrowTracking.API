using ESAM.GrowTracking.Infrastructure.Settings;
using ESAM.GrowTracking.Persistence.Settings;

namespace ESAM.GrowTracking.API.Security
{
    internal static class StartupSecurityGuard
    {
        internal static void Validate(IConfiguration configuration)
        {
            if (configuration is not IConfigurationRoot configRoot)
                return;
            var fileProviders = configRoot.Providers.Where(static p => p.GetType().FullName is string typeName &&
                (typeName.Contains("JsonConfigurationProvider", StringComparison.OrdinalIgnoreCase) ||
                typeName.Contains("XmlConfigurationProvider", StringComparison.OrdinalIgnoreCase) ||
                typeName.Contains("IniConfigurationProvider", StringComparison.OrdinalIgnoreCase)));
            foreach (var provider in fileProviders)
            {
                if (provider.TryGet($"{nameof(JwtSettings)}:{nameof(JwtSettings.SecretKey)}", out var secretFromFile) && !string.IsNullOrWhiteSpace(secretFromFile))
                    throw new InvalidOperationException($"Startup Security: '{nameof(JwtSettings)}:{nameof(JwtSettings.SecretKey)}' detectado en archivo en entorno no-Development. " +
                        "Proveer mediante variables de entorno o Secret Manager.");
                if (provider.TryGet($"ConnectionStrings:{nameof(ConnectionStringSettings.DefaultConnection)}", out var connFromFile) && !string.IsNullOrWhiteSpace(connFromFile) && 
                    (connFromFile.Contains("Password=", StringComparison.OrdinalIgnoreCase) ||  connFromFile.Contains("pwd=", StringComparison.OrdinalIgnoreCase)))
                    throw new InvalidOperationException($"Startup Security: 'ConnectionStrings:{nameof(ConnectionStringSettings.DefaultConnection)}' contiene credenciales en " + 
                        "texto plano en archivo. Use variables de entorno o Secret Manager.");
            }
        }
    }
}