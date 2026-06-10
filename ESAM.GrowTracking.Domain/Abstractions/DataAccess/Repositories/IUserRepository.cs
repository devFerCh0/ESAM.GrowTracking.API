using ESAM.GrowTracking.Domain.Entities;

namespace ESAM.GrowTracking.Domain.Abstractions.DataAccess.Repositories
{
    public interface IUserRepository : IRepository<User, int>
    {
        Task<User?> GetByCredentialAsync(string credential, bool asTracking = false, CancellationToken cancellationToken = default);

        Task<bool> ValidateCurrentUserStatusAsync(int currentUserId, DateTime utcNow, bool asTracking = false, CancellationToken cancellationToken = default);

        Task<bool> ValidateCurrentUserSecurityAsync(int currentUserId, string currenSecurityStamp, int currentTokenVersion, bool asTracking = false,
            CancellationToken cancellationToken = default);
    }
}