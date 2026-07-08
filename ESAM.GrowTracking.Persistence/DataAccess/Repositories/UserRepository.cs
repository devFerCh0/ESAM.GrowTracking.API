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
            return await query.FirstOrDefaultAsync(u => u.NormalizedUserName == normalizedCredential || u.NormalizedEmail == normalizedCredential, cancellationToken);
        }

        public async Task<bool> IsActiveAndUnlockedAsync(int id, DateTime utcNow, bool asTracking = false, CancellationToken cancellationToken = default)
        {
            var query = asTracking ? _dbSet.AsTracking() : _dbSet.AsNoTracking();
            return await query.AnyAsync(u => u.Id == id && !u.IsDeleted && (u.LockoutEndAt == null || u.LockoutEndAt <= utcNow), cancellationToken);
        }

        public async Task<bool> HasValidSecurityCredentialsAsync(int id, string securityStamp, int tokenVersion, bool asTracking = false, 
            CancellationToken cancellationToken = default)
        {
            var query = asTracking ? _dbSet.AsTracking() : _dbSet.AsNoTracking();
            return await query.AnyAsync(u => u.Id == id && u.SecurityStamp == securityStamp && u.TokenVersion == tokenVersion, cancellationToken);
        }

        public async Task<bool> ExistsAsync(int id, bool asTracking = false, CancellationToken cancellationToken = default)
        {
            var query = asTracking ? _dbSet.AsTracking() : _dbSet.AsNoTracking();
            return await query.AnyAsync(u => u.Id == id, cancellationToken);
        }
    }
}