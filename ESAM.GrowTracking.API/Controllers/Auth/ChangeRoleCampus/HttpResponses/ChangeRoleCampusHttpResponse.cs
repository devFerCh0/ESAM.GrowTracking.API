namespace ESAM.GrowTracking.API.Controllers.Auth.ChangeRoleCampus.HttpResponses
{
    public record ChangeRoleCampusHttpResponse
    {
        public string AccessToken { get; init; }

        public int AccessTokenExpiresIn { get; init; }

        public DateTime AccessTokenExpiresAt { get; init; }

        public ChangeRoleCampusUserHttpResponse ChangeRoleCampusUser { get; init; }

        public ChangeRoleCampusHttpResponse(string accessToken, int accessTokenExpiresIn, DateTime accessTokenExpiresAt, ChangeRoleCampusUserHttpResponse changeRoleCampusUser)
        {
            AccessToken = accessToken;
            AccessTokenExpiresIn = accessTokenExpiresIn;
            AccessTokenExpiresAt = accessTokenExpiresAt;
            ChangeRoleCampusUser = changeRoleCampusUser;
        }
    }
}