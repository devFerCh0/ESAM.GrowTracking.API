using ESAM.GrowTracking.Domain.Abstractions.DataAccess.Repositories;
using ESAM.GrowTracking.Domain.Entities;
using ESAM.GrowTracking.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ESAM.GrowTracking.Persistence.DataAccess.Repositories
{
    public class RolePermissionRepository(ILogger<RolePermissionRepository> logger, AppDbContext context) : Repository<RolePermission>(logger, context), IRolePermissionRepository
    {
        //public async Task<bool> HasActivePermissionsAsync(int roleId, bool asTracking = false, CancellationToken cancellationToken = default)
        //{
        //    var query = asTracking ? _dbSet.AsTracking() : _dbSet.AsNoTracking();
        //    return await query.AnyAsync(rp => rp.RoleId == roleId && rp.HasAccess && !rp.Permission.IsDeleted, cancellationToken).ConfigureAwait(false);
        //}
    }
}