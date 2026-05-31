using ESAM.GrowTracking.API.Abstractions.Mappers;
using ESAM.GrowTracking.API.Adapters;
using ESAM.GrowTracking.API.ConfigureOptions;
using ESAM.GrowTracking.API.Controllers.Auth.AssumeRoleCampus;
using ESAM.GrowTracking.API.Controllers.Auth.AssumeWorkProfile;
using ESAM.GrowTracking.API.Controllers.Auth.GetUserRoleCampuses;
using ESAM.GrowTracking.API.Controllers.Auth.Login;
using ESAM.GrowTracking.API.Controllers.Auth.Refresh;
using ESAM.GrowTracking.API.Filters;
using ESAM.GrowTracking.API.HealthChecks;
using ESAM.GrowTracking.API.Mappers;
using ESAM.GrowTracking.API.Security;
using ESAM.GrowTracking.API.Settings;
using ESAM.GrowTracking.Infrastructure.Abstractions.Http;
using ESAM.GrowTracking.Infrastructure.Abstractions.Security;
using ESAM.GrowTracking.Infrastructure.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace ESAM.GrowTracking.API
{
    public static class APIServiceExtensions
    {
        public static IServiceCollection AddAPISettings(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configuration);
            ArgumentNullException.ThrowIfNull(environment);
            services.AddOptions<DataProtectionSettings>().Bind(configuration.GetSection(nameof(DataProtectionSettings)))
                .Validate(dataProtectionSettings => { dataProtectionSettings.Validate(environment.IsProduction()); return true; }).ValidateOnStart();
            services.AddOptions<ForwardedHeadersSettings>().Bind(configuration.GetSection(nameof(ForwardedHeadersSettings)))
                .Validate(forwardedHeadersSettings => { forwardedHeadersSettings.Validate(); return true; }).ValidateOnStart();
            return services;
        }

        public static IServiceCollection AddAPIHttpContextServices(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);            
            services.AddHttpContextAccessor();
            services.AddScoped<HttpContextAdapter>();
            services.AddScoped<IClaimsPrincipalProvider>(static sp => sp.GetRequiredService<HttpContextAdapter>());
            services.AddScoped<IHttpRequestContextReader>(static sp => sp.GetRequiredService<HttpContextAdapter>());
            services.AddScoped<IResponseCookieWriter>(static sp => sp.GetRequiredService<HttpContextAdapter>());
            return services;
        }

        public static IServiceCollection AddAPIDataProtection(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);
            services.AddDataProtection().SetApplicationName("ESAM.GrowTracking");
            services.ConfigureOptions<KeyManagementOptionsSetup>();
            services.AddSingleton<ITokenProtector, DataProtectionTokenProtector>();
            return services;
        }

        public static IServiceCollection AddAPICors(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);
            services.AddCors();
            services.ConfigureOptions<CorsOptionsSetup>();
            return services;
        }

        public static IServiceCollection AddAPICookie(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);
            services.ConfigureOptions<CookiePolicyOptionsSetup>();
            return services;
        }

        public static WebApplication UseConfiguredSwagger(this WebApplication app)
        {
            ArgumentNullException.ThrowIfNull(app);
            if (!app.Environment.IsDevelopment())
                return app;
            var cookieSettings = app.Services.GetRequiredService<IOptions<CookieSettings>>().Value;
            var xsrfCookieName = cookieSettings.EffectiveXsrfCookieName();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.UseRequestInterceptor(
                    $"(req) => {{" +
                    $"  try {{" +
                    $"    const cookie = document.cookie.split('; ')" +
                    $"      .find(r => r.startsWith('{xsrfCookieName}='));" +
                    $"    if (cookie) {{" +
                    $"      req.headers['X-XSRF-TOKEN'] =" +
                    $"        decodeURIComponent(cookie.split('=').slice(1).join('='));" +
                    $"    }}" +
                    $"  }} catch (e) {{ console.warn('XSRF interceptor error', e); }}" +
                    $"  return req;" +
                    $"}}");
                c.EnablePersistAuthorization();
                c.DisplayRequestDuration();
            });
            return app;
        }

        public static IEndpointRouteBuilder MapHealthCheckEndpoints(this IEndpointRouteBuilder endpoints)
        {
            ArgumentNullException.ThrowIfNull(endpoints);
            endpoints.MapHealthChecks("/health/live", new HealthCheckOptions
            {
                Predicate = _ => false,
                ResponseWriter = HealthCheckResponseWriters.WriteLiveResponse
            }).DisableRateLimiting();
            endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains("ready"),
                ResponseWriter = HealthCheckResponseWriters.WriteReadyResponse
            }).DisableRateLimiting();
            return endpoints;
        }

        //public static IServiceCollection AddAPIMiddlewareServices(this IServiceCollection services)
        //{
        //    ArgumentNullException.ThrowIfNull(services);
        //    services.AddScoped<ValidateModelStateFilter>();
        //    services.AddSingleton<IErrorToHttpMapper, ErrorToHttpMapper>();
        //    services.AddSingleton<GlobalExceptionMiddleware>();
        //    services.AddSingleton<SecurityHeadersMiddleware>();
        //    services.AddSingleton<XsrfValidationMiddleware>();
        //    services.AddSingleton<CorrelationIdMiddleware>();
        //    return services;
        //}

        public static IServiceCollection AddAPIMiddlewareServices(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);
            services.AddProblemDetails();
            services.AddHealthChecks();
            services.AddScoped<ValidateModelStateFilter>();
            services.AddSingleton<IErrorToHttpMapper, ErrorToHttpMapper>();
            return services;
        }

        public static IServiceCollection AddAPIAuthentication(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);
            services.AddAuthentication(authenticationOptions =>
            {
                authenticationOptions.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                authenticationOptions.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer();
            services.ConfigureOptions<JwtBearerOptionsSetup>();
            return services;
        }

        public static IServiceCollection AddAPIAuthorization(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);
            services.AddAuthorization();
            return services;
        }

        public static IServiceCollection AddAPIForwardedHeaders(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);
            services.ConfigureOptions<ForwardedHeadersOptionsSetup>();
            return services;
        }

        public static IServiceCollection AddAPIRateLimiting(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);
            services.AddRateLimiter(static _ => { });
            services.ConfigureOptions<RateLimiterOptionsSetup>();
            return services;
        }

        public static IServiceCollection AddAPIResponseCompression(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);
            services.AddResponseCompression(options => { options.EnableForHttps = true; });
            return services;
        }

        public static IServiceCollection AddAPIControllers(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);
            services.AddControllers(options =>
            {
                options.Filters.AddService<ValidateModelStateFilter>(0);
            });
            services.ConfigureOptions<ApiBehaviorOptionsSetup>();
            services.ConfigureOptions<JsonOptionsSetup>();
            return services;
        }

        //public static IServiceCollection AddAPISwagger(this IServiceCollection services)
        //{
        //    ArgumentNullException.ThrowIfNull(services);
        //    services.AddEndpointsApiExplorer();
        //    services.AddSwaggerGen();
        //    services.ConfigureOptions<SwaggerGenOptionsSetup>();
        //    return services;
        //}

        public static IServiceCollection AddAPISwagger(this IServiceCollection services, IHostEnvironment environment)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(environment);
            if (!environment.IsDevelopment())
                return services;
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
            services.ConfigureOptions<SwaggerGenOptionsSetup>();
            return services;
        }

        public static IServiceCollection AddMappingProfiles(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);
            services.AddAutoMapper(mce =>
            {
                mce.AddMaps(typeof(LoginMappingProfile).Assembly);
                mce.AddMaps(typeof(AssumeWorkProfileMappingProfile).Assembly);
                mce.AddMaps(typeof(UserRoleCampusMappingProfile).Assembly);
                mce.AddMaps(typeof(AssumeRoleCampusMappingProfile).Assembly);
                mce.AddMaps(typeof(RefreshMappingProfile).Assembly);
            });
            return services;
        }
    }
}