using ESAM.GrowTracking.Domain.Abstractions.DataAccess.Repositories;
using ESAM.GrowTracking.Domain.Entities;
using ESAM.GrowTracking.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ESAM.GrowTracking.Persistence.DataAccess.Repositories
{
    public class RoleRepository(ILogger<RoleRepository> logger, AppDbContext context) : Repository<Role, int>(logger, context), IRoleRepository
    {
        public async Task<bool> IsActiveAsync(int id, bool asTracking = false, CancellationToken cancellationToken = default)
        {
            var query = asTracking ? _dbSet.AsTracking() : _dbSet.AsNoTracking();
            return await query.AnyAsync(r => r.Id == id && !r.IsDeleted, cancellationToken).ConfigureAwait(false);
        }
    }
}