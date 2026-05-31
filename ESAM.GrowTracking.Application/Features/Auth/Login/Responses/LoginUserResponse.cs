namespace ESAM.GrowTracking.Application.Features.Auth.Login.Responses
{
    public record LoginUserResponse
    {
        public int UserId { get; init; }

        public string Username { get; init; }

        public string Email { get; init; }

        public string Fullname { get; init; }

        public IReadOnlyCollection<LoginUserWorkProfileResponse> UserWorkProfiles { get; init; }

        public LoginUserResponse(int userId, string username, string email, string fullName, IReadOnlyCollection<LoginUserWorkProfileResponse> userWorkProfiles)
        {
            UserId = userId;
            Username = username;
            Email = email;
            Fullname = fullName;
            UserWorkProfiles = userWorkProfiles;
        }
    }
}