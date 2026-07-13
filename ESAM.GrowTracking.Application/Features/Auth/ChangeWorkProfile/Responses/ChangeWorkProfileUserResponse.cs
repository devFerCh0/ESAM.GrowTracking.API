namespace ESAM.GrowTracking.Application.Features.Auth.ChangeWorkProfile.Responses
{
    public record ChangeWorkProfileUserResponse
    {
        public int UserId { get; init; }

        public string Username { get; init; }

        public string Email { get; init; }

        public string Fullname { get; init; }

        public string? PhotoURL { get; init; }

        public List<ChangeWorkProfileUserWorkProfileResponse> ChangeWorkProfileUserWorkProfiles { get; init; }

        public ChangeWorkProfileUserSessionResponse? ChangeWorkProfileUserSession { get; init; }

        public ChangeWorkProfileUserResponse(int userId, string username, string email, string fullname, string? photoURL,
            List<ChangeWorkProfileUserWorkProfileResponse> changeWorkProfileUserWorkProfiles, ChangeWorkProfileUserSessionResponse? changeWorkProfileUserSession)
        {
            UserId = userId;
            Username = username;
            Email = email;
            Fullname = fullname;
            PhotoURL = photoURL;
            ChangeWorkProfileUserWorkProfiles = changeWorkProfileUserWorkProfiles;
            ChangeWorkProfileUserSession = changeWorkProfileUserSession;
        }
    }
}