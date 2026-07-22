using ESAM.GrowTracking.Application.Abstractions.Services;
using ESAM.GrowTracking.Domain.Entities;

namespace ESAM.GrowTracking.Application.Services
{
    public class UserDeviceService : IUserDeviceService
    {
        public UserDeviceService() { }

        public void UserDevicxUnlock(UserDevice userDevice, int currentUserId, DateTime utcNow)
        {
            userDevice.ResetFailedLogin(currentUserId, utcNow);
            userDevice.UpdateLastSeenAt(utcNow, currentUserId, utcNow);
        }
    }
}