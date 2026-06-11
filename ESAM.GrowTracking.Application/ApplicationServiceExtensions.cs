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
            //services.AddOptions<AuthSecuritySettings>().Bind(configuration.GetSection(nameof(AuthSecuritySettings)))
            //    .Validate(authSecuritySettings => { authSecuritySettings.Validate(isProduction); return true; }).ValidateOnStart();
            //services.AddOptions<CleanupSettings>().Bind(configuration.GetSection(nameof(CleanupSettings)))
            //    .Validate(cleanupSettings => { cleanupSettings.Validate(); return true; }).ValidateOnStart();
            //services.AddOptions<TokenLifetimeSettings>().Bind(configuration.GetSection(nameof(TokenLifetimeSettings)))
            //    .Validate(tokenLifetimeSettings => { tokenLifetimeSettings.Validate(isProduction); return true; }).ValidateOnStart();
            return services;
        }

        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);
            RegisterServices(services);
            RegisterCommandMediatR(services);
            RegisterQueriesMediatR(services);
            return services;
        }

        private static void RegisterServices(IServiceCollection services)
        {
            //services.AddScoped<IBlacklistedTokenService, BlacklistedTokenService>();
            //services.AddScoped<ICurrentUserValidatorService, CurrentUserValidatorService>();
            //services.AddScoped<IPurgeExpiredTokensService, PurgeExpiredTokensService>();
            //services.AddScoped<ITokenSessionValidationService, TokenSessionValidationService>();
            //services.AddScoped<IUserSessionService, UserSessionService>();
            //services.AddHostedService<PurgeExpiredTokensHostedService>();
        }

        private static void RegisterCommandMediatR(IServiceCollection services)
        {
            //services.AddMediatR(mrsc =>
            //{
            //    mrsc.RegisterServicesFromAssembly(typeof(LoginCommand).Assembly);
            //    mrsc.RegisterServicesFromAssembly(typeof(AssumeWorkProfileCommand).Assembly);
            //    mrsc.RegisterServicesFromAssembly(typeof(AssumeRoleCampusCommand).Assembly);
            //    mrsc.RegisterServicesFromAssembly(typeof(RefreshCommand).Assembly);
            //    mrsc.RegisterServicesFromAssembly(typeof(LogoutCommand).Assembly);
            //});
            //services.AddValidatorsFromAssemblyContaining<LoginCommandValidator>()
            //.AddValidatorsFromAssemblyContaining<AssumeWorkProfileCommandValidator>()
            //.AddValidatorsFromAssemblyContaining<AssumeRoleCampusCommandValidator>()
            //.AddValidatorsFromAssemblyContaining<RefreshCommandValidator>()
            //.AddValidatorsFromAssemblyContaining<LogoutCommandValidator>();
        }

        private static void RegisterQueriesMediatR(IServiceCollection services)
        {
            //services.AddMediatR(mrsc =>
            //{
            //    mrsc.RegisterServicesFromAssembly(typeof(GetUserRoleCampusesQuery).Assembly);
            //});
            //services.AddValidatorsFromAssemblyContaining<GetUserRoleCampusesQueryValidator>();
        }
    }
}