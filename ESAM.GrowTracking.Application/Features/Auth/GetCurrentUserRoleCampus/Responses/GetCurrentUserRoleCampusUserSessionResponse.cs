namespace ESAM.GrowTracking.Application.Features.Auth.GetCurrentUserRoleCampus.Responses
{
    public record GetCurrentUserRoleCampusUserSessionResponse
    {
        public int UserSessionId { get; init; }

        public string? IpAddress { get; init; }

        public string? UserAgent { get; init; }

        public GetCurrentUserRoleCampusSessionWorkProfileSelectedResponse? CurrentUserRoleCampusSessionWorkProfileSelected { get; init; }

        public GetCurrentUserRoleCampusUserSessionResponse(int userSessionId, string? ipAddress, string? userAgent,
            GetCurrentUserRoleCampusSessionWorkProfileSelectedResponse? currentUserRoleCampusSessionWorkProfileSelected)
        {
            UserSessionId = userSessionId;
            IpAddress = ipAddress;
            UserAgent = userAgent;
            CurrentUserRoleCampusSessionWorkProfileSelected = currentUserRoleCampusSessionWorkProfileSelected;
        }
    }
}