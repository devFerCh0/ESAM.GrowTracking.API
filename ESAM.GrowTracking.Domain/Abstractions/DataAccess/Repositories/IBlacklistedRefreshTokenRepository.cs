using ESAM.GrowTracking.Domain.Entities;

namespace ESAM.GrowTracking.Domain.Abstractions.DataAccess.Repositories
{
    public interface IBlacklistedRefreshTokenRepository : IRepository<BlacklistedRefreshToken, int>
    {
        Task<List<string>> GetExistingIdentifiersAsync(List<string> identifiers, bool asTracking = false, CancellationToken cancellationToken = default);

        Task<bool> ExistsAsync(string identifier, bool asTracking = false, CancellationToken cancellationToken = default);

        Task<int> PurgeExpiredBlacklistedRefreshTokensAsync(int batchSize, DateTime utcNow, CancellationToken cancellationToken = default);
    }
}