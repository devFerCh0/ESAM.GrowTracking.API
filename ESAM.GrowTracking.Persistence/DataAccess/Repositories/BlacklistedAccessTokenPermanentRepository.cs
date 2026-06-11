using ESAM.GrowTracking.Domain.Abstractions.DataAccess.Repositories;
using ESAM.GrowTracking.Domain.Entities;
using ESAM.GrowTracking.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ESAM.GrowTracking.Persistence.DataAccess.Repositories
{
    public class BlacklistedAccessTokenPermanentRepository(ILogger<BlacklistedAccessTokenPermanentRepository> logger, AppDbContext context) 
        : Repository<BlacklistedAccessTokenPermanent, int>(logger, context), IBlacklistedAccessTokenPermanentRepository
    {
        //public async Task<bool> ExistsAsync(string jti, bool asTracking = false, CancellationToken cancellationToken = default)
        //{
        //    var query = asTracking ? _dbSet.AsTracking() : _dbSet.AsNoTracking();
        //    return await query.AnyAsync(batp => batp.Jti == jti, cancellationToken).ConfigureAwait(false);
        //}
        
        //public async Task<int> PurgeExpiredBlacklistedAccessTokensPermanentAsync(int batchSize, DateTime utcNow, CancellationToken cancellationToken = default)
        //{
        //    if (batchSize <= 0)
        //        batchSize = 1000;
        //    var totalDeleted = 0;
        //    while (!cancellationToken.IsCancellationRequested)
        //    {
        //        var affected = await _dbSet.Where(batp => batp.ExpiresAt < utcNow).OrderBy(batp => batp.Id).Take(batchSize).ExecuteDeleteAsync(cancellationToken)
        //            .ConfigureAwait(false);
        //        totalDeleted += affected;
        //        if (affected < batchSize)
        //            break;
        //    }
        //    return totalDeleted;
        //}
    }
}