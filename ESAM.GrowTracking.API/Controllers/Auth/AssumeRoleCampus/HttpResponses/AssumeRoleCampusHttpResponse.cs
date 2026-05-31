namespace ESAM.GrowTracking.API.Controllers.Auth.AssumeRoleCampus.HttpResponses
{
    public record AssumeRoleCampusHttpResponse
    {
        public string AccessToken { get; init; }

        public int AccessTokenExpiresIn { get; init; }

        public DateTime AccessTokenExpiresAt { get; init; }

        public string RefreshTokenRaw { get; init; }

        public int RefreshTokenExpiresIn { get; init; }

        public DateTime RefreshTokenExpiresAt { get; init; }

        public AssumeRoleCampusUserHttpResponse AssumeRoleCampusUser { get; init; }

        public AssumeRoleCampusHttpResponse(string accessToken, int accessTokenExpiresIn, DateTime accessTokenExpiresAt, string identifier, string refreshToken, 
            int refreshTokenExpiresIn, DateTime refreshTokenExpiresAt, AssumeRoleCampusUserHttpResponse assumeRoleCampusUser)
        {
            AccessToken = accessToken;
            AccessTokenExpiresIn = accessTokenExpiresIn;
            AccessTokenExpiresAt = accessTokenExpiresAt;
            RefreshTokenRaw = $"{identifier}.{refreshToken}";
            RefreshTokenExpiresIn = refreshTokenExpiresIn;
            RefreshTokenExpiresAt = refreshTokenExpiresAt;
            AssumeRoleCampusUser = assumeRoleCampusUser;
        }
    }
}