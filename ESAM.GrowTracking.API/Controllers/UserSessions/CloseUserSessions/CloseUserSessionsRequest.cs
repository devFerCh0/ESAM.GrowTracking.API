namespace ESAM.GrowTracking.API.Controllers.UserSessions.CloseUserSessions
{
    public record CloseUserSessionsRequest
    {
        public int? UserId { get; init; }

        public CloseUserSessionsRequest(int? userId)
        {
            UserId = userId;
        }
    }
}