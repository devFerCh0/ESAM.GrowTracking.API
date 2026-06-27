using ESAM.GrowTracking.Domain.Entities;

namespace ESAM.GrowTracking.Domain.Abstractions.DataAccess.Repositories
{
    public interface IUserSessionRefreshTokenRepository : IRepository<UserSessionRefreshToken, int>
    {
        Task<UserSessionRefreshToken?> GetByIdentifierAsync(string identifier, bool asTracking = false, CancellationToken cancellationToken = default);

        Task<List<UserSessionRefreshToken>> GetAllByUserSessionIdAsync(int userSessionId, bool asTracking = false, CancellationToken cancellationToken = default);

        //Task<int> PurgeExpiredUserSessionRefreshTokensAsync(int batchSize, DateTime utcNow, CancellationToken cancellationToken = default);
    }
}