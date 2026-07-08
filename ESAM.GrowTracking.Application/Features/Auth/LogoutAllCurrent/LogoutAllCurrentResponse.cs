namespace ESAM.GrowTracking.Application.Features.Auth.LogoutAllCurrent
{
    public record LogoutAllCurrentResponse
    {
        public int RevokedSessionsCount { get; init; }

        public LogoutAllCurrentResponse(int revokedSessionsCount)
        {
            RevokedSessionsCount = revokedSessionsCount;
        }
    }
}