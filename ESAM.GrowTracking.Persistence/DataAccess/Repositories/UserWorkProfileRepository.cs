using ESAM.GrowTracking.Domain.Abstractions.DataAccess.Repositories;
using ESAM.GrowTracking.Domain.Entities;
using ESAM.GrowTracking.Domain.Enums;
using ESAM.GrowTracking.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ESAM.GrowTracking.Persistence.DataAccess.Repositories
{
    public class UserWorkProfileRepository(ILogger<UserWorkProfileRepository> logger, AppDbContext context) : Repository<UserWorkProfile>(logger, context), 
        IUserWorkProfileRepository
    {
        public async Task<UserWorkProfile?> GetByUserIdAndWorkProfileIdAsync(int userId, int workProfileId, bool asTracking = false, CancellationToken cancellationToken = default)
        {
            var query = asTracking ? _dbSet.AsTracking() : _dbSet.AsNoTracking();
            return await query.FirstOrDefaultAsync(uwp => uwp.UserId == userId && uwp.WorkProfileId == workProfileId, cancellationToken).ConfigureAwait(false);
        }

        public async Task<bool> IsActiveAndOfTypeAsync(int userId, int workProfileId, WorkProfileType workProfileType, bool asTracking = false, 
            CancellationToken cancellationToken = default)
        {
            var query = asTracking ? _dbSet.AsTracking() : _dbSet.AsNoTracking();
            return await query.AnyAsync(uwp => uwp.UserId == userId && uwp.WorkProfileId == workProfileId && !uwp.IsDeleted && uwp.WorkProfile.WorkProfileType == workProfileType, 
                cancellationToken).ConfigureAwait(false);
        }
    }
}