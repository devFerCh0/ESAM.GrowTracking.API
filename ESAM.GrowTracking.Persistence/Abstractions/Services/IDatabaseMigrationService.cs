namespace ESAM.GrowTracking.Persistence.Abstractions.Services
{
    public interface IDatabaseMigrationService
    {
        Task ApplyMigrationsAsync(CancellationToken cancellationToken = default);
    }
}