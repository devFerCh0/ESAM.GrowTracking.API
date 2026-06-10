namespace ESAM.GrowTracking.Application.Features.Auth.GetCurrentUserWorkProfile.Responses
{
    public record GetCurrentUserWorkProfileUserSessionResponse
    {
        public int UserSessionId { get; init; }

        public string? IpAddress { get; init; }

        public string? UserAgent { get; init; }

        public GetCurrentUserWorkProfileSessionWorkProfileSelectedResponse? CurrentUserWorkProfileSessionWorkProfileSelected { get; init; }

        public GetCurrentUserWorkProfileUserSessionResponse(int userSessionId, string? ipAddress, string? userAgent,
            GetCurrentUserWorkProfileSessionWorkProfileSelectedResponse? currentUserWorkProfileSessionWorkProfileSelected)
        {
            UserSessionId = userSessionId;
            IpAddress = ipAddress;
            UserAgent = userAgent;
            CurrentUserWorkProfileSessionWorkProfileSelected = currentUserWorkProfileSessionWorkProfileSelected;
        }
    }
}