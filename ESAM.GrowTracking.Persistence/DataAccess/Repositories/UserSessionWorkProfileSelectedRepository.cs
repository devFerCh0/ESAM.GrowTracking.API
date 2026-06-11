using ESAM.GrowTracking.Domain.Abstractions.DataAccess.Repositories;
using ESAM.GrowTracking.Domain.Entities;
using ESAM.GrowTracking.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ESAM.GrowTracking.Persistence.DataAccess.Repositories
{
    public class UserSessionWorkProfileSelectedRepository(ILogger<UserSessionWorkProfileSelectedRepository> logger, AppDbContext context) 
        : Repository<UserSessionWorkProfileSelected>(logger, context), IUserSessionWorkProfileSelectedRepository
    {
        //public async Task<UserSessionWorkProfileSelected?> GetByUserSessionIdAsync(int userSessionId, bool asTracking = false, CancellationToken cancellationToken = default)
        //{
        //    var query = asTracking ? _dbSet.AsTracking() : _dbSet.AsNoTracking();
        //    return await query.FirstOrDefaultAsync(uswps => uswps.UserSessionId == userSessionId, cancellationToken).ConfigureAwait(false);
        //}
    }
}