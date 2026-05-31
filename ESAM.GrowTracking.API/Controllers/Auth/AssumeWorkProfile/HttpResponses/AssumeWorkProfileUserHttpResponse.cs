namespace ESAM.GrowTracking.API.Controllers.Auth.AssumeWorkProfile.HttpResponses
{
    public record AssumeWorkProfileUserHttpResponse
    {
        public int UserId { get; init; }

        public string Username { get; init; }

        public string Email { get; init; }

        public string Fullname { get; init; }

        public string? PhotoURL { get; init; }

        public List<AssumeWorkProfileUserWorkProfileHttpResponse> AssumeWorkProfileUserWorkProfiles { get; init; }

        public AssumeWorkProfileUserSessionHttpResponse AssumeWorkProfileUserSession { get; init; }

        public AssumeWorkProfileUserHttpResponse(int userId, string username, string email, string fullname, string? photoURL, 
            List<AssumeWorkProfileUserWorkProfileHttpResponse> assumeWorkProfileUserWorkProfiles, AssumeWorkProfileUserSessionHttpResponse assumeWorkProfileUserSession)
        {
            UserId = userId;
            Username = username;
            Email = email;
            Fullname = fullname;
            PhotoURL = photoURL;
            AssumeWorkProfileUserWorkProfiles = assumeWorkProfileUserWorkProfiles;
            AssumeWorkProfileUserSession = assumeWorkProfileUserSession;
        }
    }
}