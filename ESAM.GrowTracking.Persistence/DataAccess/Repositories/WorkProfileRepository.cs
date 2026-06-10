using ESAM.GrowTracking.Domain.Abstractions.DataAccess.Repositories;
using ESAM.GrowTracking.Domain.Entities;
using ESAM.GrowTracking.Domain.Enums;
using ESAM.GrowTracking.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ESAM.GrowTracking.Persistence.DataAccess.Repositories
{
    public class WorkProfileRepository(ILogger<WorkProfileRepository> logger, AppDbContext context) : Repository<WorkProfile, int>(logger, context), IWorkProfileRepository
    {
        public async Task<bool> IsValidWorkProfileTypeAsync(int id, WorkProfileType workProfileType, bool asTracking = false, CancellationToken cancellationToken = default)
        {
            var query = asTracking ? _dbSet.AsTracking() : _dbSet.AsNoTracking();
            return await query.AnyAsync(wp => wp.Id == id && wp.WorkProfileType == workProfileType && !wp.IsDeleted, cancellationToken).ConfigureAwait(false);
        }

        public async Task<WorkProfileType> GetWorkProfileTypeByIdAsync(int id, bool asTracking = false, CancellationToken cancellationToken = default)
        {
            var query = asTracking ? _dbSet.AsTracking() : _dbSet.AsNoTracking();
            return await query.Where(wp => wp.Id == id && !wp.IsDeleted).Select(wp => wp.WorkProfileType).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<bool> IsActiveAndOfTypeAsync(int id, WorkProfileType workProfileType, bool asTracking = false, CancellationToken cancellationToken = default)
        {
            var query = asTracking ? _dbSet.AsTracking() : _dbSet.AsNoTracking();
            return await query.AnyAsync(wp => wp.Id == id && wp.WorkProfileType == workProfileType && !wp.IsDeleted, cancellationToken).ConfigureAwait(false);
        }
    }
}