namespace ESAM.GrowTracking.API.Controllers.Auth.RevokeCurrentUserSession
{
    public record RevokeCurrentUserSessionRequest
    {
        public int? UserSessionId { get; init; }

        public RevokeCurrentUserSessionRequest(int? userSessionId)
        {
            UserSessionId = userSessionId;
        }
    }
}