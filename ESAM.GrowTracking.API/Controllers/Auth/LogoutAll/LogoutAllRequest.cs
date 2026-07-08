namespace ESAM.GrowTracking.API.Controllers.Auth.LogoutAll
{
    public record LogoutAllRequest
    {
        public int? UserId { get; init; }

        public LogoutAllRequest(int? userId)
        {
            UserId = userId;
        }
    }
}