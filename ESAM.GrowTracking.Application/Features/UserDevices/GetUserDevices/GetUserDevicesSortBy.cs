namespace ESAM.GrowTracking.Application.Features.UserDevices.GetUserDevices
{
    public enum GetUserDevicesSortBy : byte
    {
        LastSeenAt = 0,
        LastLoginAt = 1,
        FailedLoginCount = 2,
        CreatedAt = 3
    }
}