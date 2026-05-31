using ESAM.GrowTracking.Application.Abstractions.Services.Results;

namespace ESAM.GrowTracking.Application.Abstractions.Services
{
    public interface IPurgeExpiredTokensService
    {
        Task<PurgeExpiredTokensResult> PurgeAsync(int batchSize, DateTime utcNow, CancellationToken cancellationToken = default);
    }
}