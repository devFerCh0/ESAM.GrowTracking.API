namespace ESAM.GrowTracking.Application.Features.Auth.AssumeWorkProfile.Responses
{
    public record AssumeWorkProfileUserResponse
    {
        public int UserId { get; init; }

        public string Username { get; init; }

        public string Email { get; init; }

        public string Fullname { get; init; }

        public string? PhotoURL { get; init; }

        public List<AssumeWorkProfileUserWorkProfileResponse> AssumeWorkProfileUserWorkProfiles { get; init; }

        public AssumeWorkProfileUserSessionResponse? AssumeWorkProfileUserSession { get; init; }

        public AssumeWorkProfileUserResponse(int userId, string username, string email, string fullname, string? photoURL, 
            List<AssumeWorkProfileUserWorkProfileResponse> assumeWorkProfileUserWorkProfiles, AssumeWorkProfileUserSessionResponse? assumeWorkProfileUserSession)
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