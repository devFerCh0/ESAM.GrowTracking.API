namespace ESAM.GrowTracking.API.Controllers.UserDevices.UnlockUserDevice
{
    public record UnlockUserDeviceRequest
    {
        public int? UserId { get; init; }

        public int? UserDeviceId { get; init; }

        public UnlockUserDeviceRequest(int? userId, int? userDeviceId)
        {
            UserId = userId;
            UserDeviceId = userDeviceId;
        }
    }
}