using ESAM.GrowTracking.Domain.Abstractions.DataAccess.Repositories;
using ESAM.GrowTracking.Domain.Entities;
using ESAM.GrowTracking.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ESAM.GrowTracking.Persistence.DataAccess.Repositories
{
    public class UserRoleCampusRepository(ILogger<UserRoleCampusRepository> logger, AppDbContext context) : Repository<UserRoleCampus>(logger, context), IUserRoleCampusRepository
    {
        public async Task<bool> IsActiveAsync(int userId, int roleId, int campusId, bool asTracking = false, CancellationToken cancellationToken = default)
        {
            var query = asTracking ? _dbSet.AsTracking() : _dbSet.AsNoTracking();
            return await query.AnyAsync(urc => urc.UserId == userId && urc.RoleId == roleId && urc.CampusId == campusId && !urc.IsDeleted, cancellationToken);
        }

        //public async Task<UserRoleCampus?> GetByUserIdAndRoleIdAndCampusIdAsync(int userId, int roleId, int campusId, bool asTracking = false, 
        //    CancellationToken cancellationToken = default)
        //{
        //    var query = asTracking ? _dbSet.AsTracking() : _dbSet.AsNoTracking();
        //    return await query.FirstOrDefaultAsync(urc => urc.UserId == userId && urc.RoleId == roleId && urc.CampusId == campusId, cancellationToken).ConfigureAwait(false);
        //}
    }
}