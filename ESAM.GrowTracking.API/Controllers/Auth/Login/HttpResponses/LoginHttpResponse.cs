namespace ESAM.GrowTracking.API.Controllers.Auth.Login.HttpResponses
{
    public record LoginHttpResponse
    {
        public string AccessToken { get; init; }

        public int AccessTokenExpiresIn { get; init; }

        public DateTime AccessTokenExpiresAt { get; init; }

        public LoginUserHttpResponse User { get; init; }

        public LoginHttpResponse(string accessToken, int accessTokenExpiresIn, DateTime accessTokenExpiresAt, LoginUserHttpResponse user)
        {
            AccessToken = accessToken;
            AccessTokenExpiresIn = accessTokenExpiresIn;
            AccessTokenExpiresAt = accessTokenExpiresAt;
            User = user;
        }
    }
}