namespace ESAM.GrowTracking.API.Controllers.Auth.ChangePassword
{
    public record ChangePasswordHttpResponse
    {
        public string AccessToken { get; init; }

        public int AccessTokenExpiresIn { get; init; }

        public DateTime AccessTokenExpiresAt { get; init; }

        public ChangePasswordHttpResponse(string accessToken, int accessTokenExpiresIn, DateTime accessTokenExpiresAt)
        {
            AccessToken = accessToken;
            AccessTokenExpiresIn = accessTokenExpiresIn;
            AccessTokenExpiresAt = accessTokenExpiresAt;
        }
    }
}