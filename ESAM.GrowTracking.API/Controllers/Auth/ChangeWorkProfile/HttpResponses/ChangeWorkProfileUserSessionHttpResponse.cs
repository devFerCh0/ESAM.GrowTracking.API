namespace ESAM.GrowTracking.API.Controllers.Auth.ChangeWorkProfile.HttpResponses
{
    public record ChangeWorkProfileUserSessionHttpResponse
    {
        public int UserSessionId { get; init; }

        public string? IpAddress { get; init; }

        public string? UserAgent { get; init; }

        public ChangeWorkProfileSessionWorkProfileSelectedHttpResponse ChangeWorkProfileSessionWorkProfileSelected { get; init; }

        public ChangeWorkProfileUserSessionHttpResponse(int userSessionId, string? ipAddress, string? userAgent,
            ChangeWorkProfileSessionWorkProfileSelectedHttpResponse changeWorkProfileSessionWorkProfileSelected)
        {
            UserSessionId = userSessionId;
            IpAddress = ipAddress;
            UserAgent = userAgent;
            ChangeWorkProfileSessionWorkProfileSelected = changeWorkProfileSessionWorkProfileSelected;
        }
    }
}