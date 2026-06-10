using ESAM.GrowTracking.Domain.Entities;

namespace ESAM.GrowTracking.Domain.Abstractions.DataAccess.Repositories
{
    public interface IUserDeviceRepository : IRepository<UserDevice, int>
    {
        Task<UserDevice?> GetByIdAndUserIdAsync(int id, int userId, bool asTracking = false, CancellationToken cancellationToken = default);

        Task<UserDevice?> GetByUserIdAndDeviceIdentifierAsync(int userId, string deviceIdentifier, bool asTracking = false, CancellationToken cancellationToken = default);

        Task<bool> ValidateCurrentUserDeviceStatusAsync(int currentUserDeviceId, int currentUserId, DateTime utcNow, bool asTracking = false, 
            CancellationToken cancellationToken = default);
    }
}