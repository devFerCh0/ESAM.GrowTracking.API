using ESAM.GrowTracking.Domain.Entities;

namespace ESAM.GrowTracking.Application.Abstractions.Services
{
    public interface IUserDeviceService
    {
        void UserDeviceUnlock(UserDevice userDevice, int currentUserId, DateTime utcNow);
    }
}