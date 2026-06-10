namespace ESAM.GrowTracking.API.Controllers.Auth.GetCurrentUserRoleCampus.HttpResponses
{
    public record GetCurrentUserRoleCampusUserSessionHttpResponse
    {
        public int UserSessionId { get; init; }

        public string? IpAddress { get; init; }

        public string? UserAgent { get; init; }

        public GetCurrentUserRoleCampusSessionWorkProfileSelectedHttpResponse? CurrentUserRoleCampusSessionWorkProfileSelected { get; init; }

        public GetCurrentUserRoleCampusUserSessionHttpResponse(int userSessionId, string? ipAddress, string? userAgent,
            GetCurrentUserRoleCampusSessionWorkProfileSelectedHttpResponse? currentUserRoleCampusSessionWorkProfileSelected)
        {
            UserSessionId = userSessionId;
            IpAddress = ipAddress;
            UserAgent = userAgent;
            CurrentUserRoleCampusSessionWorkProfileSelected = currentUserRoleCampusSessionWorkProfileSelected;
        }
    }
}