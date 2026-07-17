namespace ESAM.GrowTracking.API.Controllers.Auth.UnlockUserDevice
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