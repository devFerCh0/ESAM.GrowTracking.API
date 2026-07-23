namespace ESAM.GrowTracking.API.Controllers.UserSessions.CloseUserSession
{
    public record CloseUserSessionRequest
    {
        public int? UserSessionId { get; init; }

        public int? UserId { get; init; }

        public CloseUserSessionRequest(int? userSessionId, int? userId)
        {
            UserSessionId = userSessionId;
            UserId = userId;
        }
    }
}