namespace ESAM.GrowTracking.API.Controllers.Auth.GetCurrentUserWorkProfile.HttpResponses
{
    public record GetCurrentUserWorkProfileHttpResponse
    {
        public int UserId { get; init; }

        public string Username { get; init; }

        public string Email { get; init; }

        public string Fullname { get; init; }

        public string? PhotoURL { get; init; }

        public List<GetCurrentUserWorkProfileUserWorkProfileHttpResponse> CurrentUserWorkProfileUserWorkProfiles { get; init; }

        public GetCurrentUserWorkProfileUserSessionHttpResponse? CurrentUserWorkProfileUserSession { get; init; }

        public GetCurrentUserWorkProfileHttpResponse(int userId, string username, string email, string fullname, string? photoURL,
            List<GetCurrentUserWorkProfileUserWorkProfileHttpResponse> currentUserWorkProfileUserWorkProfiles,
            GetCurrentUserWorkProfileUserSessionHttpResponse? currentUserWorkProfileUserSession)
        {
            UserId = userId;
            Username = username;
            Email = email;
            Fullname = fullname;
            PhotoURL = photoURL;
            CurrentUserWorkProfileUserWorkProfiles = currentUserWorkProfileUserWorkProfiles;
            CurrentUserWorkProfileUserSession = currentUserWorkProfileUserSession;
        }
    }
}