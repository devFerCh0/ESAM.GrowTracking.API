using ESAM.GrowTracking.Domain.Abstractions.DataAccess.Repositories;
using ESAM.GrowTracking.Domain.Entities;
using ESAM.GrowTracking.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ESAM.GrowTracking.Persistence.DataAccess.Repositories
{
    public class UserRepository(ILogger<UserRepository> logger, AppDbContext context) : Repository<User, int>(logger, context), IUserRepository
    {
        public async Task<User?> GetByCredentialAsync(string credential, bool asTracking = false, CancellationToken cancellationToken = default)
        {
            var normalizedCredential = credential.Trim().ToUpperInvariant();
            var query = asTracking ? _dbSet.AsTracking() : _dbSet.AsNoTracking();
            return await query.FirstOrDefaultAsync(u => u.NormalizedUserName == normalizedCredential || u.NormalizedEmail == normalizedCredential, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}