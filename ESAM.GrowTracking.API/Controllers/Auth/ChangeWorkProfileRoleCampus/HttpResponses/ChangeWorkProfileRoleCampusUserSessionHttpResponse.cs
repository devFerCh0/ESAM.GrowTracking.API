namespace ESAM.GrowTracking.API.Controllers.Auth.ChangeWorkProfileRoleCampus.HttpResponses
{
    public record ChangeWorkProfileRoleCampusUserSessionHttpResponse
    {
        public int UserSessionId { get; init; }

        public string? IpAddress { get; init; }

        public string? UserAgent { get; init; }

        public ChangeWorkProfileRoleCampusSessionWorkProfileSelectedHttpResponse ChangeWorkProfileRoleCampusSessionWorkProfileSelected { get; init; }

        public ChangeWorkProfileRoleCampusUserSessionHttpResponse(int userSessionId, string? ipAddress, string? userAgent,
            ChangeWorkProfileRoleCampusSessionWorkProfileSelectedHttpResponse changeWorkProfileRoleCampusSessionWorkProfileSelected)
        {
            UserSessionId = userSessionId;
            IpAddress = ipAddress;
            UserAgent = userAgent;
            ChangeWorkProfileRoleCampusSessionWorkProfileSelected = changeWorkProfileRoleCampusSessionWorkProfileSelected;
        }
    }
}