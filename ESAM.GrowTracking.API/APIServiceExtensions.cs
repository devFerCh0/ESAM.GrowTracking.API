using ESAM.GrowTracking.API.Abstractions.Mappers;
using ESAM.GrowTracking.API.Abstractions.Security;
using ESAM.GrowTracking.API.Adapters;
using ESAM.GrowTracking.API.ConfigureOptions;
using ESAM.GrowTracking.API.Controllers.Auth.GetUserRoleCampuses;
using ESAM.GrowTracking.API.Controllers.Auth.Login;
using ESAM.GrowTracking.API.Filters;
using ESAM.GrowTracking.API.HealthChecks;
using ESAM.GrowTracking.API.Mappers;
using ESAM.GrowTracking.API.Security;
using ESAM.GrowTracking.API.Serialization;
using ESAM.GrowTracking.API.Settings;
using ESAM.GrowTracking.Application.Enums;
using ESAM.GrowTracking.Infrastructure.Abstractions.Http;
using ESAM.GrowTracking.Infrastructure.Abstractions.Security;
using ESAM.GrowTracking.Infrastructure.Security;
using ESAM.GrowTracking.Infrastructure.Settings;
using FluentValidation;
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

        public static IServiceCollection AddAPIHsts(this IServiceCollection services, IHostEnvironment environment)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(environment);
            if (!environment.IsDevelopment())
            {
                services.AddHsts(options =>
                {
                    options.MaxAge = TimeSpan.FromSeconds(15552000);
                    options.IncludeSubDomains = true;
                    options.Preload = true;
                });
            }
            return services;
        }

        public static WebApplication UseConfiguredSwagger(this WebApplication app)
        {
            ArgumentNullException.ThrowIfNull(app);
            if (!app.Environment.IsDevelopment())
                return app;
            app.UseSwagger();
            app.UseSwaggerUI(suio =>
            {
                suio.InjectJavascript("/swagger-ui/custom.js");
                suio.EnablePersistAuthorization();
                suio.DisplayRequestDuration();
            });
            return app;
        }

        public static WebApplication MapSwaggerUiCustomJs(this WebApplication app)
        {
            ArgumentNullException.ThrowIfNull(app);
            if (!app.Environment.IsDevelopment())
                return app;
            app.MapGet("/swagger-ui/custom.js", (IOptions<CookieSettings> cookieSettings) =>
            {
                var xsrfCookieName = cookieSettings.Value.EffectiveXsrfCookieName();
                var js = $$"""
                function initConfig(configObject, oauthConfigObject)
                {
                    configObject.requestInterceptor = function (req)
                    {
                        try
                        {
                            const cookieName = '{{xsrfCookieName}}';
                            const cookie = document.cookie.split('; ').find(function (value) { return value.startsWith(cookieName + '='); });
                            if (cookie)
                            {
                                req.headers['X-XSRF-TOKEN'] = decodeURIComponent(cookie.split('=').slice(1).join('='));
                            }
                        }
                        catch (e)
                        {
                            console.warn('XSRF interceptor error', e);
                        }
                        return req;
                    };
                }
                """;
                return Results.Text(js, "application/javascript");
            }).AllowAnonymous();
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

        public static IServiceCollection AddAPIMiddlewareServices(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);
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
            services.AddScoped<IJwtTokenValidatedHandler, JwtTokenValidatedHandler>();
            services.ConfigureOptions<JwtBearerOptionsSetup>();
            return services;
        }

        public static IServiceCollection AddAPIAuthorization(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);
            services.AddAuthorizationBuilder()
                .AddPolicy(AuthorizationPolicies.RequireSessionTypeAccessToken, policy => policy.RequireClaim(CustomClaimTypes.AccessTokenType, 
                    AccessTokenType.Session.GetStringValue()))
                .AddPolicy(AuthorizationPolicies.RequireTemporaryTypeAccessToken, policy => policy.RequireClaim(CustomClaimTypes.AccessTokenType, 
                    AccessTokenType.Temporary.GetStringValue()));
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

        public static IServiceCollection AddAPIMappingProfiles(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);
            services.AddAutoMapper(mce =>
            {
                mce.AddMaps(typeof(LoginMappingProfile).Assembly);
                mce.AddMaps(typeof(GetUserRoleCampusMappingProfile).Assembly);
                //    mce.AddMaps(typeof(AssumeWorkProfileMappingProfile).Assembly);
                //    mce.AddMaps(typeof(AssumeRoleCampusMappingProfile).Assembly);
                //    mce.AddMaps(typeof(RefreshMappingProfile).Assembly);
                //    mce.AddMaps(typeof(GetCurrentUserWorkProfileMappingProfile).Assembly);
                //    mce.AddMaps(typeof(GetCurrentUserRoleCampusMappingProfile).Assembly);
            });
            return services;
        }

        public static IServiceCollection AddAPIValidators(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);
            services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();
            return services;
        }
    }
}