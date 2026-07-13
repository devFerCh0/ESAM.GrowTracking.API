namespace ESAM.GrowTracking.Application.Features.Auth.ChangeWorkProfile.Responses
{
    public record ChangeWorkProfileResponse
    {
        public string AccessToken { get; init; }

        public int AccessTokenExpiresIn { get; init; }

        public DateTime AccessTokenExpiresAt { get; init; }

        public ChangeWorkProfileUserResponse ChangeWorkProfileUser { get; init; }

        public ChangeWorkProfileResponse(string accessToken, int accessTokenExpiresIn, DateTime accessTokenExpiresAt, ChangeWorkProfileUserResponse changeWorkProfileUser)
        {
            AccessToken = accessToken;
            AccessTokenExpiresIn = accessTokenExpiresIn;
            AccessTokenExpiresAt = accessTokenExpiresAt;
            ChangeWorkProfileUser = changeWorkProfileUser;
        }
    }
}