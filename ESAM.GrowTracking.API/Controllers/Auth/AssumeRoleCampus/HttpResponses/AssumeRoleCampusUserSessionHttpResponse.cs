namespace ESAM.GrowTracking.API.Controllers.Auth.AssumeRoleCampus.HttpResponses
{
    public record AssumeRoleCampusUserSessionHttpResponse
    {
        public int UserSessionId { get; init; }

        public string? IpAddress { get; init; }

        public string? UserAgent { get; init; }

        public AssumeRoleCampusSessionWorkProfileSelectedHttpResponse AssumeRoleCampusSessionWorkProfileSelected { get; init; }

        public AssumeRoleCampusUserSessionHttpResponse(int userSessionId, string? ipAddress, string? userAgent,
            AssumeRoleCampusSessionWorkProfileSelectedHttpResponse assumeRoleCampusSessionWorkProfileSelected)
        {
            UserSessionId = userSessionId;
            IpAddress = ipAddress;
            UserAgent = userAgent;
            AssumeRoleCampusSessionWorkProfileSelected = assumeRoleCampusSessionWorkProfileSelected;
        }
    }
}