using ESAM.GrowTracking.Domain.Abstractions.DataAccess.Repositories;
using ESAM.GrowTracking.Domain.Entities;
using ESAM.GrowTracking.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ESAM.GrowTracking.Persistence.DataAccess.Repositories
{
    public class WorkProfilePermissionRepository(ILogger<WorkProfilePermissionRepository> logger, AppDbContext context) : Repository<WorkProfilePermission>(logger, context), 
        IWorkProfilePermissionRepository
    {
        public async Task<bool> HasActivePermissionsWithAccessAsync(int workProfileId, bool asTracking = false, CancellationToken cancellationToken = default)
        {
            var query = asTracking ? _dbSet.AsTracking() : _dbSet.AsNoTracking();
            return await query.AnyAsync(wpp => wpp.WorkProfileId == workProfileId && wpp.HasAccess && !wpp.Permission.IsDeleted, cancellationToken);
        }
    }
}