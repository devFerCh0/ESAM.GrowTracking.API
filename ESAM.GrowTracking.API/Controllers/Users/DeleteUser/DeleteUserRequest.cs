namespace ESAM.GrowTracking.API.Controllers.Users.DeleteUser
{
    public record DeleteUserRequest
    {
        public int? UserId { get; init; }

        public DeleteUserRequest(int? userId)
        {
            UserId = userId;
        }
    }
}