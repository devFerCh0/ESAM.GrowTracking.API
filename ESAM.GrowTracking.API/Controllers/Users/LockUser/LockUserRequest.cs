namespace ESAM.GrowTracking.API.Controllers.Users.LockUser
{
    public record LockUserRequest
    {
        public int? UserId { get; init; }

        public DateTime? LockoutEndAt { get; init; }

        public LockUserRequest(int? userId, DateTime? lockoutEndAt)
        {
            UserId = userId;
            LockoutEndAt = lockoutEndAt;
        }
    }
}