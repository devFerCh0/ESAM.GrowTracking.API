namespace ESAM.GrowTracking.API.Controllers.Auth.LogoutAllCurrent
{
    public record LogoutAllCurrentHttpResponse
    {
        public int RevokedSessionsCount { get; init; }

        public LogoutAllCurrentHttpResponse(int revokedSessionsCount)
        {
            RevokedSessionsCount = revokedSessionsCount;
        }
    }
}