namespace ESAM.GrowTracking.Application.Features.Auth.AssumeRoleCampus.Responses
{
    public record AssumeRoleCampusResponse
    {
        public string AccessToken { get; init; }

        public int AccessTokenExpiresIn { get; init; }

        public DateTime AccessTokenExpiresAt { get; init; }

        public string RefreshTokenRaw { get; init; }

        public int RefreshTokenExpiresIn { get; init; }

        public DateTime RefreshTokenExpiresAt { get; init; }

        public AssumeRoleCampusUserResponse AssumeRoleCampusUser { get; init; }

        public AssumeRoleCampusResponse(string accessToken, int accessTokenExpiresIn, DateTime accessTokenExpiresAt, string identifier, string refreshToken, 
            int refreshTokenExpiresIn, DateTime refreshTokenExpiresAt, AssumeRoleCampusUserResponse assumeRoleCampusUser)
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