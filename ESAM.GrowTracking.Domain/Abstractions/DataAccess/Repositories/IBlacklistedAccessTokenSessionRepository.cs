using ESAM.GrowTracking.Domain.Entities;

namespace ESAM.GrowTracking.Domain.Abstractions.DataAccess.Repositories
{
    public interface IBlacklistedAccessTokenSessionRepository : IRepository<BlacklistedAccessTokenSession, int>
    {
        Task<bool> DoesNotExistAsync(string jti, bool asTracking = false, CancellationToken cancellationToken = default);

        //Task<bool> ExistsAsync(string jti, bool asTracking = false, CancellationToken cancellationToken = default);

        //Task<int> PurgeExpiredBlacklistedAccessTokensPermanentAsync(int batchSize, DateTime utcNow, CancellationToken cancellationToken = default);
    }
}