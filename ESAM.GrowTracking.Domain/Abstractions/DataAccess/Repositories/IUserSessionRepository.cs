using ESAM.GrowTracking.Domain.Entities;

namespace ESAM.GrowTracking.Domain.Abstractions.DataAccess.Repositories
{
    public interface IUserSessionRepository : IRepository<UserSession, int>
    {
        Task<bool> IsUnRevokedAndUnExpiredForWorkProfileAsync(int id, int userId, int workProfileId, DateTime utcNow, bool asTracking = false, 
            CancellationToken cancellationToken = default);

        Task<bool> IsUnRevokedAndUnExpiredForRoleCampusAsync(int id, int userId, int workProfileId, int roleId, int campusId, DateTime utcNow, bool asTracking = false, 
            CancellationToken cancellationToken = default);

        Task<UserSession?> GetByIdAndUserIdAndUserDeviceIdAsync(int id, int userId, int userDeviceId, bool asTracking = false, CancellationToken cancellationToken = default);
        
        Task<UserSession?> GetByIdAndUserIdAsync(int id, int userId, bool asTracking = false, CancellationToken cancellationToken = default);
    }
}