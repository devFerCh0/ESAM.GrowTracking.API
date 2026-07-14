namespace ESAM.GrowTracking.API.Controllers.Auth.ChangeWorkProfileRoleCampus.HttpResponses
{
    public record ChangeWorkProfileRoleCampusHttpResponse
    {
        public string AccessToken { get; init; }

        public int AccessTokenExpiresIn { get; init; }

        public DateTime AccessTokenExpiresAt { get; init; }

        public ChangeWorkProfileRoleCampusUserHttpResponse ChangeWorkProfileRoleCampusUser { get; init; }

        public ChangeWorkProfileRoleCampusHttpResponse(string accessToken, int accessTokenExpiresIn, DateTime accessTokenExpiresAt, 
            ChangeWorkProfileRoleCampusUserHttpResponse changeWorkProfileRoleCampusUser)
        {
            AccessToken = accessToken;
            AccessTokenExpiresIn = accessTokenExpiresIn;
            AccessTokenExpiresAt = accessTokenExpiresAt;
            ChangeWorkProfileRoleCampusUser = changeWorkProfileRoleCampusUser;
        }
    }
}