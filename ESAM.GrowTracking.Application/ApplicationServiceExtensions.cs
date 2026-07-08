using ESAM.GrowTracking.Application.Abstractions.Services;
using ESAM.GrowTracking.Application.Features.Auth.AssumeRoleCampus;
using ESAM.GrowTracking.Application.Features.Auth.AssumeWorkProfile;
using ESAM.GrowTracking.Application.Features.Auth.GetActiveCurrentUserSessions;
using ESAM.GrowTracking.Application.Features.Auth.GetActiveUserSessions;
using ESAM.GrowTracking.Application.Features.Auth.GetCurrentUserRoleCampus;
using ESAM.GrowTracking.Application.Features.Auth.GetCurrentUserWorkProfile;
using ESAM.GrowTracking.Application.Features.Auth.GetUserRoleCampuses;
using ESAM.GrowTracking.Application.Features.Auth.Login;
using ESAM.GrowTracking.Application.Features.Auth.Logout;
using ESAM.GrowTracking.Application.Features.Auth.LogoutAll;
using ESAM.GrowTracking.Application.Features.Auth.LogoutAllCurrent;
using ESAM.GrowTracking.Application.Features.Auth.Refresh;
using ESAM.GrowTracking.Application.Features.Auth.RevokeCurrentUserSession;
using ESAM.GrowTracking.Application.Features.Auth.RevokeUserSession;
using ESAM.GrowTracking.Application.Services;
using ESAM.GrowTracking.Application.Settings;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ESAM.GrowTracking.Application
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationSettings(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configuration);
            ArgumentNullException.ThrowIfNull(environment);
            var isProduction = environment.IsProduction();
            services.AddOptions<AuthSecuritySettings>().Bind(configuration.GetSection(nameof(AuthSecuritySettings)))
                .Validate(authSecuritySettings => { authSecuritySettings.Validate(isProduction); return true; }).ValidateOnStart();
            //services.AddOptions<CleanupSettings>().Bind(configuration.GetSection(nameof(CleanupSettings)))
            //    .Validate(cleanupSettings => { cleanupSettings.Validate(); return true; }).ValidateOnStart();
            services.AddOptions<TokenLifetimeSettings>().Bind(configuration.GetSection(nameof(TokenLifetimeSettings)))
                .Validate(tokenLifetimeSettings => { tokenLifetimeSettings.Validate(isProduction); return true; }).ValidateOnStart();
            return services;
        }

        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);
            RegisterServices(services);
            RegisterMediatR(services);
            RegisterValidators(services);
            return services;
        }

        private static void RegisterServices(IServiceCollection services)
        {
            services.AddScoped<IAccessTokenClaimsValidatorService, AccessTokenClaimsValidatorService>();
            services.AddScoped<ISecurityValidatorService, SecurityValidatorService>();
            services.AddScoped<IUserSessionService, UserSessionService>();
            services.AddScoped<IAuthSessionIntegrityValidatorService, AuthSessionIntegrityValidatorService>();

            //services.AddScoped<IBlacklistedTokenService, BlacklistedTokenService>();
            //services.AddScoped<ICurrentUserValidatorService, CurrentUserValidatorService>();
            //services.AddScoped<IPurgeExpiredTokensService, PurgeExpiredTokensService>();
            //services.AddScoped<ITokenSessionValidationService, TokenSessionValidationService>();
            //services.AddHostedService<PurgeExpiredTokensHostedService>();
        }

        private static void RegisterMediatR(IServiceCollection services)
        {
            services.AddMediatR(mrsc =>
            {
                mrsc.RegisterServicesFromAssembly(typeof(LoginCommand).Assembly);
                mrsc.RegisterServicesFromAssembly(typeof(GetUserRoleCampusesQuery).Assembly);
                mrsc.RegisterServicesFromAssembly(typeof(AssumeRoleCampusCommand).Assembly);
                mrsc.RegisterServicesFromAssembly(typeof(AssumeWorkProfileCommand).Assembly);
                mrsc.RegisterServicesFromAssembly(typeof(GetCurrentUserRoleCampusQuery).Assembly);
                mrsc.RegisterServicesFromAssembly(typeof(GetCurrentUserWorkProfileQuery).Assembly);
                mrsc.RegisterServicesFromAssembly(typeof(LogoutCommand).Assembly);
                mrsc.RegisterServicesFromAssembly(typeof(RefreshCommand).Assembly);
                mrsc.RegisterServicesFromAssembly(typeof(GetActiveUserSessionsQuery).Assembly);
                mrsc.RegisterServicesFromAssembly(typeof(GetActiveCurrentUserSessionsQuery).Assembly);
                mrsc.RegisterServicesFromAssembly(typeof(RevokeUserSessionCommand).Assembly);
                mrsc.RegisterServicesFromAssembly(typeof(RevokeCurrentUserSessionCommand).Assembly);
                mrsc.RegisterServicesFromAssembly(typeof(LogoutAllCommand).Assembly);
                mrsc.RegisterServicesFromAssembly(typeof(LogoutAllCurrentCommand).Assembly);
            });
        }

        private static void RegisterValidators(IServiceCollection services)
        {
            services.AddValidatorsFromAssemblyContaining<LoginCommandValidator>();
            services.AddValidatorsFromAssemblyContaining<GetUserRoleCampusesQueryValidator>();
            services.AddValidatorsFromAssemblyContaining<AssumeRoleCampusCommandValidator>();
            services.AddValidatorsFromAssemblyContaining<AssumeWorkProfileCommandValidator>();
            services.AddValidatorsFromAssemblyContaining<LogoutCommandValidator>();
            services.AddValidatorsFromAssemblyContaining<RefreshCommandValidator>();
            services.AddValidatorsFromAssemblyContaining<GetActiveUserSessionsQueryValidator>();
            services.AddValidatorsFromAssemblyContaining<RevokeUserSessionCommandValidator>();
            services.AddValidatorsFromAssemblyContaining<RevokeCurrentUserSessionCommandValidator>();
            services.AddValidatorsFromAssemblyContaining<LogoutAllCommandValidator>();
        }
    }
}