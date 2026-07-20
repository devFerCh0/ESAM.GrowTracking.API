namespace ESAM.GrowTracking.Application.Features.Users.LogoutAll
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