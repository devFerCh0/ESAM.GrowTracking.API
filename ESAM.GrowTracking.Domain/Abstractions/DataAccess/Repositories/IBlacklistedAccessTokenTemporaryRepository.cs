using ESAM.GrowTracking.Domain.Entities;

namespace ESAM.GrowTracking.Domain.Abstractions.DataAccess.Repositories
{
    public interface IBlacklistedAccessTokenTemporaryRepository : IRepository<BlacklistedAccessTokenTemporary, int>
    {
        Task<bool> ExistsAsync(string jti, bool asTracking = false, CancellationToken cancellationToken = default);

        Task<int> PurgeExpiredBlacklistedAccessTokensTemporaryAsync(int batchSize, DateTime utcNow, CancellationToken cancellationToken = default);
    }
}