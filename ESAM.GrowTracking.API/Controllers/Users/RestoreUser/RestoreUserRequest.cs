namespace ESAM.GrowTracking.API.Controllers.Users.RestoreUser
{
    public record RestoreUserRequest
    {
        public int? UserId { get; init; }

        public RestoreUserRequest(int? userId)
        {
            UserId = userId;
        }
    }
}