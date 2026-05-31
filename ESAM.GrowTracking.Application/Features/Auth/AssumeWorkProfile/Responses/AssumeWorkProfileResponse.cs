namespace ESAM.GrowTracking.Application.Features.Auth.AssumeWorkProfile.Responses
{
    public record AssumeWorkProfileResponse
    {
        public string AccessToken { get; init; }

        public int AccessTokenExpiresIn { get; init; }

        public DateTime AccessTokenExpiresAt { get; init; }

        public string RefreshTokenRaw { get; init; }

        public int RefreshTokenExpiresIn { get; init; }

        public DateTime RefreshTokenExpiresAt { get; init; }

        public AssumeWorkProfileUserResponse AssumeWorkProfileUser { get; init; }

        public AssumeWorkProfileResponse(string accessToken, int accessTokenExpiresIn, DateTime accessTokenExpiresAt, string identifier, string refreshToken, 
            int refreshTokenExpiresIn, DateTime refreshTokenExpiresAt, AssumeWorkProfileUserResponse assumeWorkProfileUser)
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