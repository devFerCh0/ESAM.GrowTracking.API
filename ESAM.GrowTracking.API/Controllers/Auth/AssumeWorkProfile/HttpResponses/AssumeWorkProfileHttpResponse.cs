namespace ESAM.GrowTracking.API.Controllers.Auth.AssumeWorkProfile.HttpResponses
{
    public record AssumeWorkProfileHttpResponse
    {
        public string AccessToken { get; init; }

        public int AccessTokenExpiresIn { get; init; }

        public DateTime AccessTokenExpiresAt { get; init; }

        public string RefreshTokenRaw { get; init; }

        public int RefreshTokenExpiresIn { get; init; }

        public DateTime RefreshTokenExpiresAt { get; init; }

        public AssumeWorkProfileUserHttpResponse AssumeWorkProfileUser { get; init; }

        public AssumeWorkProfileHttpResponse(string accessToken, int accessTokenExpiresIn, DateTime accessTokenExpiresAt, string identifier, string refreshToken, 
            int refreshTokenExpiresIn, DateTime refreshTokenExpiresAt, AssumeWorkProfileUserHttpResponse assumeWorkProfileUser)
        {
            AccessToken = accessToken;
            AccessTokenExpiresIn = accessTokenExpiresIn;
            AccessTokenExpiresAt = accessTokenExpiresAt;
            RefreshTokenRaw = $"{identifier}.{refreshToken}";
            RefreshTokenExpiresIn = refreshTokenExpiresIn;
            RefreshTokenExpiresAt = refreshTokenExpiresAt;
            AssumeWorkProfileUser = assumeWorkProfileUser;
        }
    }
}