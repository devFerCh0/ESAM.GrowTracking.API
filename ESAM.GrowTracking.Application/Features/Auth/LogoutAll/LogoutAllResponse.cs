namespace ESAM.GrowTracking.Application.Features.Auth.LogoutAll
{
    public record LogoutAllResponse
    {
        public int RevokedSessionsCount { get; init; }

        public LogoutAllResponse(int revokedSessionsCount)
        {
            RevokedSessionsCount = revokedSessionsCount;
        }
    }
}