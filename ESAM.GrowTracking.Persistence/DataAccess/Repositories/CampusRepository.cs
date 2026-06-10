using ESAM.GrowTracking.Domain.Abstractions.DataAccess.Repositories;
using ESAM.GrowTracking.Domain.Entities;
using ESAM.GrowTracking.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ESAM.GrowTracking.Persistence.DataAccess.Repositories
{
    public class CampusRepository(ILogger<CampusRepository> logger, AppDbContext context) : Repository<Campus, int>(logger, context), ICampusRepository
    {
        public async Task<bool> IsActiveAsync(int id, bool asTracking = false, CancellationToken cancellationToken = default)
        {
            var query = asTracking ? _dbSet.AsTracking() : _dbSet.AsNoTracking();
            return await query.AnyAsync(c => c.Id == id && !c.IsDeleted, cancellationToken).ConfigureAwait(false);
        }
    }
}