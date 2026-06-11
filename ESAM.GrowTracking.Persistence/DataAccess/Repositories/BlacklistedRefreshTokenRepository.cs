using ESAM.GrowTracking.Domain.Abstractions.DataAccess.Repositories;
using ESAM.GrowTracking.Domain.Entities;
using ESAM.GrowTracking.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ESAM.GrowTracking.Persistence.DataAccess.Repositories
{
    public class BlacklistedRefreshTokenRepository(ILogger<BlacklistedRefreshTokenRepository> logger, AppDbContext context) 
        : Repository<BlacklistedRefreshToken, int>(logger, context), IBlacklistedRefreshTokenRepository
    {
        //public async Task<List<string>> GetExistingIdentifiersAsync(List<string> identifiers, bool asTracking = false, CancellationToken cancellationToken = default)
        //{
        //    var query = asTracking ? _dbSet.AsTracking() : _dbSet.AsNoTracking();
        //    return await query.Where(brt => identifiers.Contains(brt.Identifier)).Select(brt => brt.Identifier).ToListAsync(cancellationToken).ConfigureAwait(false);
        //}

        //public async Task<bool> ExistsAsync(string identifier, bool asTracking = false, CancellationToken cancellationToken = default)
        //{
        //    var query = asTracking ? _dbSet.AsTracking() : _dbSet.AsNoTracking();
        //    return await query.AnyAsync(brt => brt.Identifier == identifier, cancellationToken).ConfigureAwait(false);
        //}

        //public async Task<int> PurgeExpiredBlacklistedRefreshTokensAsync(int batchSize, DateTime utcNow, CancellationToken cancellationToken = default)
        //{
        //    if (batchSize <= 0)
        //        batchSize = 1000;
        //    var totalDeleted = 0;
        //    while (!cancellationToken.IsCancellationRequested)
        //    {
        //        var affected = await _dbSet.Where(brt => brt.ExpiresAt < utcNow).OrderBy(brt => brt.Id).Take(batchSize).ExecuteDeleteAsync(cancellationToken)
        //            .ConfigureAwait(false);
        //        totalDeleted += affected;
        //        if (affected < batchSize)
        //            break;
        //    }
        //    return totalDeleted;
        //}
    }
}