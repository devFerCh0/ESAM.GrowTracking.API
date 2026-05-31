namespace ESAM.GrowTracking.API.Controllers.Auth.Refresh
{
    public record RefreshHttpResponse
    {
        public string AccessToken { get; init; }

        public int AccessTokenExpiresIn { get; init; }

        public DateTime AccessTokenExpiresAt { get; init; }

        public string RefreshTokenRaw { get; init; }

        public int RefreshTokenExpiresIn { get; init; }

        public DateTime RefreshTokenExpiresAt { get; init; }

        public RefreshHttpResponse(string accessToken, int accessTokenExpiresIn, DateTime accessTokenExpiresAt, string identifier, string refreshToken, int refreshTokenExpiresIn, 
            DateTime refreshTokenExpiresAt)
        {
            AccessToken = accessToken;
            AccessTokenExpiresIn = accessTokenExpiresIn;
            AccessTokenExpiresAt = accessTokenExpiresAt;
            RefreshTokenRaw = $"{identifier}.{refreshToken}";
            RefreshTokenExpiresIn = refreshTokenExpiresIn;
            RefreshTokenExpiresAt = refreshTokenExpiresAt;
        }
    }
}