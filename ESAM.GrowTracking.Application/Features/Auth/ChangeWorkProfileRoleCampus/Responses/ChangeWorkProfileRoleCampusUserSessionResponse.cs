namespace ESAM.GrowTracking.Application.Features.Auth.ChangeWorkProfileRoleCampus.Responses
{
    public record ChangeWorkProfileRoleCampusUserSessionResponse
    {
        public int UserSessionId { get; init; }

        public string? IpAddress { get; init; }

        public string? UserAgent { get; init; }

        public ChangeWorkProfileRoleCampusSessionWorkProfileSelectedResponse? ChangeWorkProfileRoleCampusSessionWorkProfileSelected { get; init; }

        public ChangeWorkProfileRoleCampusUserSessionResponse(int userSessionId, string? ipAddress, string? userAgent,
            ChangeWorkProfileRoleCampusSessionWorkProfileSelectedResponse? changeWorkProfileRoleCampusSessionWorkProfileSelected)
        {
            UserSessionId = userSessionId;
            IpAddress = ipAddress;
            UserAgent = userAgent;
            ChangeWorkProfileRoleCampusSessionWorkProfileSelected = changeWorkProfileRoleCampusSessionWorkProfileSelected;
        }
    }
}