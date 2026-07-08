namespace ESAM.GrowTracking.API.Controllers.Auth.RevokeUserSession
{
    public record RevokeUserSessionRequest
    {
        public int? UserSessionId { get; init; }

        public int? UserId { get; init; }

        public RevokeUserSessionRequest(int? userSessionId, int? userId)
        {
            UserSessionId = userSessionId;
            UserId = userId;
        }
    }
}