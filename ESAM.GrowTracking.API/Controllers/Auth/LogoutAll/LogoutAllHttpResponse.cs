namespace ESAM.GrowTracking.API.Controllers.Auth.LogoutAll
{
    public record LogoutAllHttpResponse
    {
        public int RevokedSessionsCount { get; init; }

        public LogoutAllHttpResponse(int revokedSessionsCount)
        {
            RevokedSessionsCount = revokedSessionsCount;
        }
    }
}