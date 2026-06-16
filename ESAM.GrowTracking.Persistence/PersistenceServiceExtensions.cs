using ESAM.GrowTracking.Application.Abstractions.DataAccess;
using ESAM.GrowTracking.Application.Abstractions.DataAccess.Queries;
using ESAM.GrowTracking.Domain.Abstractions.DataAccess;
using ESAM.GrowTracking.Domain.Abstractions.DataAccess.Repositories;
using ESAM.GrowTracking.Persistence.Abstractions.Services;
using ESAM.GrowTracking.Persistence.Contexts;
using ESAM.GrowTracking.Persistence.DataAccess;
using ESAM.GrowTracking.Persistence.DataAccess.Queries;
using ESAM.GrowTracking.Persistence.DataAccess.Repositories;
using ESAM.GrowTracking.Persistence.Services;
using ESAM.GrowTracking.Persistence.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ESAM.GrowTracking.Persistence
{
    public static class PersistenceServiceExtensions
    {
        public static IServiceCollection AddPersistenceSettings(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configuration);
            ArgumentNullException.ThrowIfNull(environment);
            var isProduction = environment.IsProduction();
            services.AddOptions<ConnectionStringSettings>().Bind(configuration.GetSection("ConnectionStrings"))
                .Validate<ILogger<ConnectionStringSettings>>((connectionStringSettings, logger) =>
                {
                    connectionStringSettings.Validate(isProduction);
                    if (isProduction && (connectionStringSettings.DefaultConnection.Contains("Integrated Security=True", StringComparison.OrdinalIgnoreCase) ||
                        connectionStringSettings.DefaultConnection.Contains("Trusted_Connection=True", StringComparison.OrdinalIgnoreCase) ||
                        connectionStringSettings.DefaultConnection.Contains("Trusted_Connection=yes", StringComparison.OrdinalIgnoreCase)))
                        logger.LogWarning("'Integrated Security' detectado en producción. Verifique privilegios mínimos.");
                    return true;
                }).ValidateOnStart();
            services.AddOptions<DatabaseSettings>().Bind(configuration.GetSection(nameof(DatabaseSettings)))
                .Validate(databaseSettings => { databaseSettings.Validate(isProduction); return true; }).ValidateOnStart();
            return services;
        }

        public static IServiceCollection AddPersistenceServices(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);
            RegisterDbContext(services);
            services.AddHealthChecks().AddDbContextCheck<AppDbContext>(name: "database", tags: ["ready"]);
            services.AddSingleton<IDatabaseMigrationService, DatabaseMigrationService>();
            RegisterRepositories(services);
            RegisterQueries(services);
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            return services;
        }

        private static void RegisterDbContext(IServiceCollection services)
        {
            services.AddDbContextPool<AppDbContext>((serviceProvider, dbContextOptionsBuilder) =>
            {
                var connectionStringSettings = serviceProvider.GetRequiredService<IOptions<ConnectionStringSettings>>().Value;
                dbContextOptionsBuilder.UseSqlServer(connectionStringSettings.DefaultConnection, sqlServerDbContextOptionsBuilder =>
                {
                    sqlServerDbContextOptionsBuilder.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
                    sqlServerDbContextOptionsBuilder.CommandTimeout(60);
                    sqlServerDbContextOptionsBuilder.MigrationsAssembly(typeof(AppDbContext).Assembly.GetName().Name);
                });
                dbContextOptionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            });
        }

        private static void RegisterRepositories(IServiceCollection services)
        {
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped(typeof(IRepository<,>), typeof(Repository<,>));
            services.AddScoped<IBlacklistedAccessTokenSessionRepository, BlacklistedAccessTokenSessionRepository>();
            services.AddScoped<IBlacklistedAccessTokenTemporaryRepository, BlacklistedAccessTokenTemporaryRepository>();
            services.AddScoped<IBlacklistedRefreshTokenRepository, BlacklistedRefreshTokenRepository>();
            services.AddScoped<IRolePermissionRepository, RolePermissionRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserDeviceRepository, UserDeviceRepository>();
            services.AddScoped<IUserRoleCampusRepository, UserRoleCampusRepository>();
            services.AddScoped<IUserSessionRepository, UserSessionRepository>();
            services.AddScoped<IUserSessionRefreshTokenRepository, UserSessionRefreshTokenRepository>();
            services.AddScoped<IUserSessionWorkProfileSelectedRepository, UserSessionWorkProfileSelectedRepository>();
            services.AddScoped<IUserSessionRoleCampusSelectedRepository, UserSessionRoleCampusSelectedRepository>();
            services.AddScoped<IUserWorkProfileRepository, UserWorkProfileRepository>();
            services.AddScoped<IWorkProfileRepository, WorkProfileRepository>();
            services.AddScoped<IWorkProfilePermissionRepository, WorkProfilePermissionRepository>();
        }

        private static void RegisterQueries(IServiceCollection services)
        {
            services.AddScoped(typeof(IQuery<>), typeof(Query<>));
            services.AddScoped(typeof(IQuery<,>), typeof(Query<,>));
            services.AddScoped<IUserQuery, UserQuery>();
            services.AddScoped<IUserRoleCampusQuery, UserRoleCampusQuery>();
        }
    }
}