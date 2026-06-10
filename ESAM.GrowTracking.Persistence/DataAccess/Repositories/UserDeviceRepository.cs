using ESAM.GrowTracking.Domain.Abstractions.DataAccess.Repositories;
using ESAM.GrowTracking.Domain.Entities;
using ESAM.GrowTracking.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ESAM.GrowTracking.Persistence.DataAccess.Repositories
{
    public class UserDeviceRepository(ILogger<UserDeviceRepository> logger, AppDbContext context) : Repository<UserDevice, int>(logger, context), IUserDeviceRepository
    {
        public async Task<UserDevice?> GetByUserIdAndDeviceIdentifierAsync(int userId, string deviceIdentifier, bool asTracking = false, 
            CancellationToken cancellationToken = default)
        {
            var query = asTracking ? _dbSet.AsTracking() : _dbSet.AsNoTracking();
            return await query.FirstOrDefaultAsync(ud => ud.UserId == userId && ud.DeviceIdentifier == deviceIdentifier, cancellationToken).ConfigureAwait(false);
        }

        public async Task<UserDevice?> GetByIdAndUserIdAsync(int id, int userId, bool asTracking = false, CancellationToken cancellationToken = default)
        {
            var query = asTracking ? _dbSet.AsTracking() : _dbSet.AsNoTracking();
            return await query.FirstOrDefaultAsync(ud => ud.Id == id && ud.UserId == userId, cancellationToken).ConfigureAwait(false);
        }

        public async Task<bool> IsActiveAndUnlockedAsync(int id, int userId, DateTime utcNow, bool asTracking = false, CancellationToken cancellationToken = default)
        {
            var query = asTracking ? _dbSet.AsTracking() : _dbSet.AsNoTracking();
            return await query.AnyAsync(ud => ud.Id == id && ud.UserId == userId && !ud.IsDeleted && (ud.LockoutEndAt == null || ud.LockoutEndAt <= utcNow), cancellationToken)
                .ConfigureAwait(false);
        }
    }
}