using ESAM.GrowTracking.Domain.Abstractions.DataAccess.Repositories;
using ESAM.GrowTracking.Domain.Entities;
using ESAM.GrowTracking.Persistence.Contexts;
using Microsoft.Extensions.Logging;

namespace ESAM.GrowTracking.Persistence.DataAccess.Repositories
{
    public class UserSessionRoleCampusSelectedRepository(ILogger<UserSessionRoleCampusSelectedRepository> logger, AppDbContext context)
        : Repository<UserSessionRoleCampusSelected, int>(logger, context), IUserSessionRoleCampusSelectedRepository { }

    //public class UserSessionRoleCampusSelectedRepository(ILogger<UserSessionRoleCampusSelectedRepository> logger, AppDbContext context) 
    //    : Repository<UserSessionRoleCampusSelected>(logger, context), IUserSessionRoleCampusSelectedRepository
    //{
    //    //public async Task<UserSessionRoleCampusSelected?> GetByUserSessionIdAsync(int userSessionId, bool asTracking = false, CancellationToken cancellationToken = default)
    //    //{
    //    //    var query = asTracking ? _dbSet.AsTracking() : _dbSet.AsNoTracking();
    //    //    return await query.FirstOrDefaultAsync(usrcs => usrcs.UserSessionId == userSessionId, cancellationToken).ConfigureAwait(false);
    //    //}
    //}
}