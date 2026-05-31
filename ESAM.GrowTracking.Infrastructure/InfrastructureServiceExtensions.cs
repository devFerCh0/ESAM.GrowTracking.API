using ESAM.GrowTracking.Infrastructure.Settings;
using ESAM.GrowTracking.Infrastructure.Validators;
using ESAM.GrowTracking.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ESAM.GrowTracking.Application.Abstractions.Services;
using ESAM.GrowTracking.Infrastructure.Abstractions.Validators;

namespace ESAM.GrowTracking.Infrastructure
{
    public static class InfrastructureServiceExtensions
    {
        public static IServiceCollection AddInfrastructureSettings(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configuration);
            ArgumentNullException.ThrowIfNull(environment);
            var isProduction = environment.IsProduction();
            services.AddOptions<ClientInfoSettings>().Bind(configuration.GetSection(nameof(ClientInfoSettings))).Validate<ILogger<ClientInfoSettings>>((clientInfoSettings, logger) =>
            {
                clientInfoSettings.Validate();
                if (isProduction && !clientInfoSettings.IpHeaderKeys.Contains("X-Forwarded-For", StringComparer.OrdinalIgnoreCase))
                    logger.LogWarning("SEGURIDAD ClientInfoSettings: 'X-Forwarded-For' no está en IpHeaderKeys. Asegúrese de que su Proxy inverso esté configurado correctamente.");
                return true;
            }).ValidateOnStart();
            services.AddOptions<JwtSettings>().Bind(configuration.GetSection(nameof(JwtSettings)))
                .Validate(jwtSettings => { jwtSettings.Validate(isProduction); return true; }).ValidateOnStart();
            services.AddOptions<CookieSettings>().Bind(configuration.GetSection(nameof(CookieSettings))).Validate<ILogger<CookieSettings>>((cookieSettings, logger) =>
            {
                cookieSettings.Validate(isProduction);
                if (isProduction && !cookieSettings.UseHostPrefix)
                    logger.LogWarning($"{nameof(cookieSettings.UseHostPrefix)}=false detectado en producción. Se recomienda el uso de prefijos __Host- para máxima seguridad " +
                        $"(obliga a {nameof(cookieSettings.AlwaysSecure)}, {nameof(cookieSettings.Path)}=/ y sin {nameof(cookieSettings.Domain)}).");
                if (isProduction && cookieSettings.AllowRefreshTokenHeader)
                    logger.LogWarning($"SEGURIDAD CookieSettings: '{nameof(cookieSettings.AllowRefreshTokenHeader)}=true' detectado en producción. " +
                        "Aceptar el refresh token vía encabezado HTTP expone mayor superficie de ataque ante XSS. " +
                        "Se recomienda deshabilitarlo y depender exclusivamente de la cookie HttpOnly.");
                return true;
            }).ValidateOnStart();
            services.AddOptions<CorsSettings>().Bind(configuration.GetSection(nameof(CorsSettings)))
                .Validate(corsSettings => { corsSettings.Validate(isProduction); return true; }).ValidateOnStart();
            return services;
        }

        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);
            RegisterScopedServices(services);
            RegisterSingletonServices(services);
            return services;
        }

        private static void RegisterScopedServices(IServiceCollection services)
        {
            services.AddScoped<IAuthCookieService, AuthCookieService>();
            services.AddScoped<IAuthSessionCookieService, AuthSessionCookieService>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<IClientInfoService, ClientInfoService>();
        }

        private static void RegisterSingletonServices(IServiceCollection services)
        {
            services.AddSingleton<ICorsOriginValidator, CorsOriginValidator>();
            services.AddSingleton<IIpAddressValidator, IpAddressValidator>();
            services.AddSingleton<IDateTimeService, DateTimeService>();
            services.AddSingleton<IHashService>(static sp => new HashService(logger: sp.GetRequiredService<ILogger<HashService>>()));
            services.AddSingleton<ITokenService, TokenService>();
        }
    }
}