namespace ESAM.GrowTracking.Application.Features.Auth.ChangeWorkProfile.Responses
{
    public record ChangeWorkProfileUserSessionResponse
    {
        public int UserSessionId { get; init; }

        public string? IpAddress { get; init; }

        public string? UserAgent { get; init; }

        public ChangeWorkProfileSessionWorkProfileSelectedResponse? ChangeWorkProfileSessionWorkProfileSelected { get; init; }

        public ChangeWorkProfileUserSessionResponse(int userSessionId, string? ipAddress, string? userAgent,
            ChangeWorkProfileSessionWorkProfileSelectedResponse? changeWorkProfileSessionWorkProfileSelected)
        {
            UserSessionId = userSessionId;
            IpAddress = ipAddress;
            UserAgent = userAgent;
            ChangeWorkProfileSessionWorkProfileSelected = changeWorkProfileSessionWorkProfileSelected;
        }
    }
}