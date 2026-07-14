namespace ESAM.GrowTracking.Application.Features.Auth.ChangeWorkProfileRoleCampus.Responses
{
    public record ChangeWorkProfileRoleCampusResponse
    {
        public string AccessToken { get; init; }

        public int AccessTokenExpiresIn { get; init; }

        public DateTime AccessTokenExpiresAt { get; init; }

        public ChangeWorkProfileRoleCampusUserResponse ChangeWorkProfileRoleCampusUser { get; init; }

        public ChangeWorkProfileRoleCampusResponse(string accessToken, int accessTokenExpiresIn, DateTime accessTokenExpiresAt, 
            ChangeWorkProfileRoleCampusUserResponse changeWorkProfileRoleCampusUser)
        {
            AccessToken = accessToken;
            AccessTokenExpiresIn = accessTokenExpiresIn;
            AccessTokenExpiresAt = accessTokenExpiresAt;
            ChangeWorkProfileRoleCampusUser = changeWorkProfileRoleCampusUser;
        }
    }
}