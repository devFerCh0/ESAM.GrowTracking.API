using ESAM.GrowTracking.Domain.Abstractions.DataAccess.Repositories;
using ESAM.GrowTracking.Domain.Entities;
using ESAM.GrowTracking.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ESAM.GrowTracking.Persistence.DataAccess.Repositories
{
    public class UserSessionRefreshTokenRepository(ILogger<UserSessionRefreshTokenRepository> logger, AppDbContext context) 
        : Repository<UserSessionRefreshToken, int>(logger, context), IUserSessionRefreshTokenRepository
    {
        public async Task<UserSessionRefreshToken?> GetByIdentifierAsync(string identifier, bool asTracking = false, CancellationToken cancellationToken = default)
        {
            var query = asTracking ? _dbSet.AsTracking() : _dbSet.AsNoTracking();
            return await query.FirstOrDefaultAsync(usrt => usrt.Identifier == identifier, cancellationToken);
        }

        public async Task<List<UserSessionRefreshToken>> GetAllByUserSessionIdAsync(int userSessionId, bool asTracking = false, CancellationToken cancellationToken = default)
        {
            var query = asTracking ? _dbSet.AsTracking() : _dbSet.AsNoTracking();
            return await query.Where(usrt => usrt.UserSessionId == userSessionId).ToListAsync(cancellationToken);
        }

        //public async Task<int> PurgeExpiredUserSessionRefreshTokensAsync(int batchSize, DateTime utcNow, CancellationToken cancellationToken = default)
        //{
        //    if (batchSize <= 0)
        //        batchSize = 1000;
        //    var totalDeleted = 0;
        //    while (!cancellationToken.IsCancellationRequested)
        //    {
        //        var affected = await _dbSet.Where(usrt => usrt.ExpiresAt < utcNow).OrderBy(usrt => usrt.Id).Take(batchSize).ExecuteDeleteAsync(cancellationToken)
        //            .ConfigureAwait(false);
        //        totalDeleted += affected;
        //        if (affected < batchSize)
        //            break;
        //    }
        //    return totalDeleted;
        //}
    }
}