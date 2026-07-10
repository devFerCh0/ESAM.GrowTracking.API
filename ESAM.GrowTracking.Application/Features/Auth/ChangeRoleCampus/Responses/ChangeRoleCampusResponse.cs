namespace ESAM.GrowTracking.Application.Features.Auth.ChangeRoleCampus.Responses
{
    public record ChangeRoleCampusResponse
    {
        public string AccessToken { get; init; }

        public int AccessTokenExpiresIn { get; init; }

        public DateTime AccessTokenExpiresAt { get; init; }

        public ChangeRoleCampusUserResponse ChangeRoleCampusUser { get; init; }

        public ChangeRoleCampusResponse(string accessToken, int accessTokenExpiresIn, DateTime accessTokenExpiresAt, ChangeRoleCampusUserResponse changeRoleCampusUser)
        {
            AccessToken = accessToken;
            AccessTokenExpiresIn = accessTokenExpiresIn;
            AccessTokenExpiresAt = accessTokenExpiresAt;
            ChangeRoleCampusUser = changeRoleCampusUser;
        }
    }
}