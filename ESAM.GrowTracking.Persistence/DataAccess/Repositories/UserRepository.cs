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

        public async Task<bool> ValidateCurrentUserStatusAsync(int currentUserId, DateTime utcNow, bool asTracking = false, CancellationToken cancellationToken = default)
        {
            var query = asTracking ? _dbSet.AsTracking() : _dbSet.AsNoTracking();
            return await query.AnyAsync(u => u.Id == currentUserId && !u.IsDeleted && (u.LockoutEndAt == null || u.LockoutEndAt <= utcNow), cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<bool> ValidateCurrentUserSecurityAsync(int currentUserId, string currenSecurityStamp, int currentTokenVersion, bool asTracking = false, 
            CancellationToken cancellationToken = default)
        {
            var query = asTracking ? _dbSet.AsTracking() : _dbSet.AsNoTracking();
            return await query.AnyAsync(u => u.Id == currentUserId && u.SecurityStamp == currenSecurityStamp && u.TokenVersion == currentTokenVersion, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}