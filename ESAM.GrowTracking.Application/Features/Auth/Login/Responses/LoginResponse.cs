namespace ESAM.GrowTracking.Application.Features.Auth.Login.Responses
{
    public record LoginResponse
    {
        public string AccessToken { get; init; }

        public int AccessTokenExpiresIn { get; init; }

        public DateTime AccessTokenExpiresAt { get; init; }

        public LoginUserResponse User { get; init; }

        public LoginResponse(string accessToken, int accessTokenExpiresIn, DateTime accessTokenExpiresAt, LoginUserResponse user)
        {
            AccessToken = accessToken;
            AccessTokenExpiresIn = accessTokenExpiresIn;
            AccessTokenExpiresAt = accessTokenExpiresAt;
            User = user;
        }
    }
}