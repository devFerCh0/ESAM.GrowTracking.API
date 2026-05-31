namespace ESAM.GrowTracking.Application.Features.Auth.AssumeRoleCampus.Responses
{
    public record AssumeRoleCampusUserSessionResponse
    {
        public int UserSessionId { get; init; }

        public string? IpAddress { get; init; }

        public string? UserAgent { get; init; }

        public AssumeRoleCampusSessionWorkProfileSelectedResponse? AssumeRoleCampusSessionWorkProfileSelected { get; init; }

        public AssumeRoleCampusUserSessionResponse(int userSessionId, string? ipAddress, string? userAgent,
            AssumeRoleCampusSessionWorkProfileSelectedResponse? assumeRoleCampusSessionWorkProfileSelected)
        {
            UserSessionId = userSessionId;
            IpAddress = ipAddress;
            UserAgent = userAgent;
            AssumeRoleCampusSessionWorkProfileSelected = assumeRoleCampusSessionWorkProfileSelected;
        }
    }
}