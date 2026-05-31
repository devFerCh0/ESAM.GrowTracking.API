using ESAM.GrowTracking.Application.Abstractions.DataAccess.Queries;
using ESAM.GrowTracking.Application.Features.Auth.GetUserRoleCampuses;
using ESAM.GrowTracking.Domain.Entities;
using ESAM.GrowTracking.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ESAM.GrowTracking.Persistence.DataAccess.Queries
{
    public class UserRoleCampusQuery(ILogger<UserRoleCampusQuery> logger, AppDbContext context) : Query<UserRoleCampus>(logger, context), IUserRoleCampusQuery
    {
        public async Task<List<UserRoleCampusResponse>> GetUserRoleCampusesByUserIdAsync(int userId, bool asTracking = false, 
            CancellationToken cancellationToken = default)
        {
            var query = asTracking ? _dbSet.AsTracking() : _dbSet.AsNoTracking();
            return await query.Where(urc => urc.UserId == userId && !urc.IsDeleted && urc.Role.RolePermissions.Any(rp => rp.HasAccess && !rp.Permission.IsDeleted))
                .Select(urc => new UserRoleCampusResponse(urc.RoleId, urc.Role.Name, urc.CampusId, urc.Campus.Name)).ToListAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}