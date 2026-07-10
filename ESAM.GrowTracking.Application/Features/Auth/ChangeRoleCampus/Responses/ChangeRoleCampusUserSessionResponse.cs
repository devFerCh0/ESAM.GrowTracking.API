namespace ESAM.GrowTracking.Application.Features.Auth.ChangeRoleCampus.Responses
{
    public record ChangeRoleCampusUserSessionResponse
    {
        public int UserSessionId { get; init; }

        public string? IpAddress { get; init; }

        public string? UserAgent { get; init; }

        public ChangeRoleCampusSessionWorkProfileSelectedResponse? ChangeRoleCampusSessionWorkProfileSelected { get; init; }

        public ChangeRoleCampusUserSessionResponse(int userSessionId, string? ipAddress, string? userAgent,
            ChangeRoleCampusSessionWorkProfileSelectedResponse? changeRoleCampusSessionWorkProfileSelected)
        {
            UserSessionId = userSessionId;
            IpAddress = ipAddress;
            UserAgent = userAgent;
            ChangeRoleCampusSessionWorkProfileSelected = changeRoleCampusSessionWorkProfileSelected;
        }
    }
}