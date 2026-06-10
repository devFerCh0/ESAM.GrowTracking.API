namespace ESAM.GrowTracking.API.Controllers.Auth.GetCurrentUserWorkProfile.HttpResponses
{
    public record GetCurrentUserWorkProfileUserSessionHttpResponse
    {
        public int UserSessionId { get; init; }

        public string? IpAddress { get; init; }

        public string? UserAgent { get; init; }

        public GetCurrentUserWorkProfileSessionWorkProfileSelectedHttpResponse? CurrentUserWorkProfileSessionWorkProfileSelected { get; init; }

        public GetCurrentUserWorkProfileUserSessionHttpResponse(int userSessionId, string? ipAddress, string? userAgent,
            GetCurrentUserWorkProfileSessionWorkProfileSelectedHttpResponse? currentUserWorkProfileSessionWorkProfileSelected)
        {
            UserSessionId = userSessionId;
            IpAddress = ipAddress;
            UserAgent = userAgent;
            CurrentUserWorkProfileSessionWorkProfileSelected = currentUserWorkProfileSessionWorkProfileSelected;
        }
    }
}