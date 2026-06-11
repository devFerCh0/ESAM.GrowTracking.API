using ESAM.GrowTracking.Domain.Abstractions.DataAccess.Repositories;
using ESAM.GrowTracking.Domain.Entities;
using ESAM.GrowTracking.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ESAM.GrowTracking.Persistence.DataAccess.Repositories
{
    public class UserSessionRepository(ILogger<UserSessionRepository> logger, AppDbContext context) : Repository<UserSession, int>(logger, context), IUserSessionRepository
    {
        //public async Task<UserSession?> GetByIdAndUserIdAndUserDeviceIdAsync(int id, int userId, int userDeviceId, bool asTracking = false, 
        //    CancellationToken cancellationToken = default)
        //{
        //    var query = asTracking ? _dbSet.AsTracking() : _dbSet.AsNoTracking();
        //    return await query.FirstOrDefaultAsync(us => us.Id == id && us.UserId == userId && us.UserDeviceId == userDeviceId, cancellationToken).ConfigureAwait(false);
        //}
    }
}