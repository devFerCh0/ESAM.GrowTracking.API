namespace ESAM.GrowTracking.API.Controllers.Auth.ChangeWorkProfile.HttpResponses
{
    public record ChangeWorkProfileHttpResponse
    {
        public string AccessToken { get; init; }

        public int AccessTokenExpiresIn { get; init; }

        public DateTime AccessTokenExpiresAt { get; init; }

        public ChangeWorkProfileUserHttpResponse ChangeWorkProfileUser { get; init; }

        public ChangeWorkProfileHttpResponse(string accessToken, int accessTokenExpiresIn, DateTime accessTokenExpiresAt, ChangeWorkProfileUserHttpResponse changeWorkProfileUser)
        {
            AccessToken = accessToken;
            AccessTokenExpiresIn = accessTokenExpiresIn;
            AccessTokenExpiresAt = accessTokenExpiresAt;
            ChangeWorkProfileUser = changeWorkProfileUser;
        }
    }
}