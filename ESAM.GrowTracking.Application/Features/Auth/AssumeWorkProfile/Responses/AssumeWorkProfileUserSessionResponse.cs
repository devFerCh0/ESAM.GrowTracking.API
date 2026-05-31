namespace ESAM.GrowTracking.Application.Features.Auth.AssumeWorkProfile.Responses
{
    public record AssumeWorkProfileUserSessionResponse
    {
        public int UserSessionId { get; init; }

        public string? IpAddress { get; init; }

        public string? UserAgent { get; init; }

        public AssumeWorkProfileSessionWorkProfileSelectedResponse? AssumeWorkProfileSessionWorkProfileSelected { get; init; }

        public AssumeWorkProfileUserSessionResponse(int userSessionId, string? ipAddress, string? userAgent,
            AssumeWorkProfileSessionWorkProfileSelectedResponse? assumeWorkProfileSessionWorkProfileSelected)
        {
            UserSessionId = userSessionId;
            IpAddress = ipAddress;
            UserAgent = userAgent;
            AssumeWorkProfileSessionWorkProfileSelected = assumeWorkProfileSessionWorkProfileSelected;
        }
    }
}