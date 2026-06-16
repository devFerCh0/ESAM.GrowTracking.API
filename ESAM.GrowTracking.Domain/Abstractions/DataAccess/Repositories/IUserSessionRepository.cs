using ESAM.GrowTracking.Domain.Entities;

namespace ESAM.GrowTracking.Domain.Abstractions.DataAccess.Repositories
{
    public interface IUserSessionRepository : IRepository<UserSession, int>
    {
        Task<bool> IsUnRevokedAndUnExpiredAsync(int currentUserSessionId, int currentUserId, DateTime utcNow, bool asTracking = false, CancellationToken cancellationToken = default);

        //Task<UserSession?> GetByIdAndUserIdAndUserDeviceIdAsync(int id, int userId, int userDeviceId, bool asTracking = false, CancellationToken cancellationToken = default);
    }
}