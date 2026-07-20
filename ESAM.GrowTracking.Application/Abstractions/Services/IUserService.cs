using ESAM.GrowTracking.Domain.Entities;

namespace ESAM.GrowTracking.Application.Abstractions.Services
{
    public interface IUserService
    {
        //Task LockUserAsync(User user, string revokedReason, int currentUserId, DateTime utcNow, bool asTracking = false, CancellationToken cancellationToken = default);
        void UserLock(User user, DateTime lockoutEndAt, int currentUserId, DateTime utcNow);

        void UserUnlock(User user, int currentUserId, DateTime utcNow);
    }
}