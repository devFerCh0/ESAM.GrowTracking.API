namespace ESAM.GrowTracking.Application.Features.Auth.ChangePassword
{
    public record ChangePasswordResponse
    {
        public string AccessToken { get; init; }

        public int AccessTokenExpiresIn { get; init; }

        public DateTime AccessTokenExpiresAt { get; init; }

        public ChangePasswordResponse(string accessToken, int accessTokenExpiresIn, DateTime accessTokenExpiresAt)
        {
            AccessToken = accessToken;
            AccessTokenExpiresIn = accessTokenExpiresIn;
            AccessTokenExpiresAt = accessTokenExpiresAt;
        }
    }
}