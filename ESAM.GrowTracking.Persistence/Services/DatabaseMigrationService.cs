using ESAM.GrowTracking.Persistence.Abstractions.Services;
using ESAM.GrowTracking.Persistence.Contexts;
using ESAM.GrowTracking.Persistence.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ESAM.GrowTracking.Persistence.Services
{
    internal sealed class DatabaseMigrationService : IDatabaseMigrationService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<DatabaseMigrationService> _logger;
        private readonly DatabaseSettings _databaseSettings;

        public DatabaseMigrationService(IServiceScopeFactory serviceScopeFactory, ILogger<DatabaseMigrationService> logger, IOptions<DatabaseSettings> databaseSettingsOptions)
        {
            ArgumentNullException.ThrowIfNull(serviceScopeFactory);
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(databaseSettingsOptions);
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
            _databaseSettings = databaseSettingsOptions.Value ?? throw new ArgumentNullException(nameof(databaseSettingsOptions));
        }

        public async Task ApplyMigrationsAsync(CancellationToken cancellationToken = default)
        {
            if (_databaseSettings.AutoMigrate != true)
                return;
            using var scope = _serviceScopeFactory.CreateScope();
            try
            {
                var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                await appDbContext.Database.MigrateAsync(cancellationToken);
                _logger.LogInformation("ApplyMigrationsAsync: migraciones aplicadas correctamente (AutoMigrate = true).");
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "ApplyMigrationsAsync: error crítico al aplicar las migraciones durante el inicio.");
                throw;
            }
        }
    }
}