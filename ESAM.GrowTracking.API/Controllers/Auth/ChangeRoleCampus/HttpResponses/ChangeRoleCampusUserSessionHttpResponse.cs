namespace ESAM.GrowTracking.API.Controllers.Auth.ChangeRoleCampus.HttpResponses
{
    public record ChangeRoleCampusUserSessionHttpResponse
    {
        public int UserSessionId { get; init; }

        public string? IpAddress { get; init; }

        public string? UserAgent { get; init; }

        public ChangeRoleCampusSessionWorkProfileSelectedHttpResponse ChangeRoleCampusSessionWorkProfileSelected { get; init; }

        public ChangeRoleCampusUserSessionHttpResponse(int userSessionId, string? ipAddress, string? userAgent,
            ChangeRoleCampusSessionWorkProfileSelectedHttpResponse changeRoleCampusSessionWorkProfileSelected)
        {
            UserSessionId = userSessionId;
            IpAddress = ipAddress;
            UserAgent = userAgent;
            ChangeRoleCampusSessionWorkProfileSelected = changeRoleCampusSessionWorkProfileSelected;
        }
    }
}