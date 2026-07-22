using ESAM.GrowTracking.Domain.Entities;

namespace ESAM.GrowTracking.Domain.Abstractions.DataAccess.Repositories
{
    public interface IUserRepository : IRepository<User, int>
    {
        Task<User?> GetByCredentialAsync(string credential, bool asTracking = false, CancellationToken cancellationToken = default);

        Task<bool> IsActiveAndUnlockedAsync(int id, DateTime utcNow, bool asTracking = false, CancellationToken cancellationToken = default);

        Task<bool> HasValidSecurityCredentialsAsync(int id, string securityStamp, int tokenVersion, bool asTracking = false, CancellationToken cancellationToken = default);
        
        Task<bool> ExistsAsync(int id, bool asTracking = false, CancellationToken cancellationToken = default);

        Task<bool> IsUserValidAsync(int id, bool asTracking = false, CancellationToken cancellationToken = default);

        //Task<bool> IsUserValidAsync(int id, DateTime utcNow, bool asTracking = false, CancellationToken cancellationToken = default);
    }
}