namespace ESAM.GrowTracking.API.Controllers.Auth.Refresh
{
    public record RefreshRequest
    {
        public string? AccessToken { get; init; }

        public string? RefreshTokenRaw { get; init; }

        public RefreshRequest(string? accessToken, string? refreshTokenRaw)
        {
            AccessToken = accessToken;
            RefreshTokenRaw = refreshTokenRaw;
        }
    }
}