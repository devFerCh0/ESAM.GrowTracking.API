namespace ESAM.GrowTracking.API.Controllers.Auth.Login.HttpResponses
{
    public record LoginUserHttpResponse
    {
        public int UserId { get; init; }

        public string Username { get; init; }

        public string Email { get; init; }

        public string Fullname { get; init; }

        public IReadOnlyCollection<LoginUserWorkProfileHttpResponse> UserWorkProfiles { get; init; }

        public LoginUserHttpResponse(int userId, string username, string email, string fullName, IReadOnlyCollection<LoginUserWorkProfileHttpResponse> userWorkProfiles)
        {
            UserId = userId;
            Username = username;
            Email = email;
            Fullname = fullName;
            UserWorkProfiles = userWorkProfiles;
        }
    }
}