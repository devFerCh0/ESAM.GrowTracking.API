namespace ESAM.GrowTracking.API.Controllers.Auth.Logout
{
    public record LogoutRequest
    {
        public string? RefreshTokenRaw { get; init; }

        public string? DeviceIdentifier { get; init; }

        public LogoutRequest(string? refreshTokenRaw, string? deviceIdentifier)
        {
            RefreshTokenRaw = refreshTokenRaw;
            DeviceIdentifier = deviceIdentifier;
        }
    }
}