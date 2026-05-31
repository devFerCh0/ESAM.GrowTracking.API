namespace ESAM.GrowTracking.API.Controllers.Auth.Refresh
{
    public record RefreshRequest
    {
        public string? RefreshTokenRaw { get; init; }

        public string? DeviceIdentifier { get; init; }

        public RefreshRequest(string? refreshTokenRaw, string? deviceIdentifier)
        {
            RefreshTokenRaw = refreshTokenRaw;
            DeviceIdentifier = deviceIdentifier;
        }
    }
}