namespace ESAM.GrowTracking.API.Controllers.Auth.AssumeWorkProfile.HttpResponses
{
    public record AssumeWorkProfileUserSessionHttpResponse
    {
        public int UserSessionId { get; init; }

        public string? IpAddress { get; init; }

        public string? UserAgent { get; init; }

        public AssumeWorkProfileSessionWorkProfileSelectedHttpResponse AssumeWorkProfileSessionWorkProfileSelected { get; init; }

        public AssumeWorkProfileUserSessionHttpResponse(int userSessionId, string? ipAddress, string? userAgent,
            AssumeWorkProfileSessionWorkProfileSelectedHttpResponse assumeWorkProfileSessionWorkProfileSelected)
        {
            UserSessionId = userSessionId;
            IpAddress = ipAddress;
            UserAgent = userAgent;
            AssumeWorkProfileSessionWorkProfileSelected = assumeWorkProfileSessionWorkProfileSelected;
        }
    }
}