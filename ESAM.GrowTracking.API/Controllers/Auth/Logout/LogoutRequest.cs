namespace ESAM.GrowTracking.API.Controllers.Auth.Logout
{
    public record LogoutRequest
    {
        public string? RefreshTokenRaw { get; init; }

        public LogoutRequest(string? refreshTokenRaw)
        {
            RefreshTokenRaw = refreshTokenRaw;
        }
    }
}