namespace ESAM.GrowTracking.API.Controllers.Auth.ChangeWorkProfile.HttpResponses
{
    public record ChangeWorkProfileUserHttpResponse
    {
        public int UserId { get; init; }

        public string Username { get; init; }

        public string Email { get; init; }

        public string Fullname { get; init; }

        public string? PhotoURL { get; init; }

        public List<ChangeWorkProfileUserWorkProfileHttpResponse> ChangeWorkProfileUserWorkProfiles { get; init; }

        public ChangeWorkProfileUserSessionHttpResponse ChangeWorkProfileUserSession { get; init; }

        public ChangeWorkProfileUserHttpResponse(int userId, string username, string email, string fullname, string? photoURL,
            List<ChangeWorkProfileUserWorkProfileHttpResponse> changeWorkProfileUserWorkProfiles, ChangeWorkProfileUserSessionHttpResponse changeWorkProfileUserSession)
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