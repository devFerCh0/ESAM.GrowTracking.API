namespace ESAM.GrowTracking.Application.Features.Auth.GetCurrentUserWorkProfile.Responses
{
    public record GetCurrentUserWorkProfileResponse
    {
        public int UserId { get; init; }

        public string Username { get; init; }

        public string Email { get; init; }

        public string Fullname { get; init; }

        public string? PhotoURL { get; init; }

        public List<GetCurrentUserWorkProfileUserWorkProfileResponse> CurrentUserWorkProfileUserWorkProfiles { get; init; }

        public GetCurrentUserWorkProfileUserSessionResponse? CurrentUserWorkProfileUserSession { get; init; }

        public GetCurrentUserWorkProfileResponse(int userId, string username, string email, string fullname, string? photoURL,
            List<GetCurrentUserWorkProfileUserWorkProfileResponse> currentUserWorkProfileUserWorkProfiles, 
            GetCurrentUserWorkProfileUserSessionResponse? currentUserWorkProfileUserSession)
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