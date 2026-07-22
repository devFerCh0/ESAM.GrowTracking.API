using ESAM.GrowTracking.Domain.Entities;

namespace ESAM.GrowTracking.Application.Abstractions.Services
{
    public interface IUserDeviceService
    {
        void UserDevicxUnlock(UserDevice userDevice, int currentUserId, DateTime utcNow);
    }
}