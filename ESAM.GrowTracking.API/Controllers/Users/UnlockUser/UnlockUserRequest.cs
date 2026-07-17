namespace ESAM.GrowTracking.API.Controllers.Users.UnlockUser
{
    public record UnlockUserRequest
    {
        public int? UserId { get; init; }

        public UnlockUserRequest(int? userId)
        {
            UserId = userId;
        }
    }
}