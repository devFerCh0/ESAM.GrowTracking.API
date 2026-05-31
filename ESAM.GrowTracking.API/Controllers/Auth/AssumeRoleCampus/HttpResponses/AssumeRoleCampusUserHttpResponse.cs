namespace ESAM.GrowTracking.API.Controllers.Auth.AssumeRoleCampus.HttpResponses
{
    public record AssumeRoleCampusUserHttpResponse
    {
        public int UserId { get; init; }

        public string Username { get; init; }

        public string Email { get; init; }

        public string Fullname { get; init; }

        public string? PhotoURL { get; init; }

        public List<AssumeRoleCampusUserWorkProfileHttpResponse> AssumeRoleCampusUserWorkProfiles { get; init; }

        public List<AssumeRoleCampusUserRoleCampusHttpResponse> AssumeRoleCampusUserRoleCampuses { get; init; }

        public AssumeRoleCampusUserSessionHttpResponse? AssumeRoleCampusUserSession { get; init; }

        public AssumeRoleCampusUserHttpResponse(int userId, string username, string email, string fullname, string? photoURL, 
            List<AssumeRoleCampusUserWorkProfileHttpResponse> assumeRoleCampusUserWorkProfiles, List<AssumeRoleCampusUserRoleCampusHttpResponse> assumeRoleCampusUserRoleCampuses,
            AssumeRoleCampusUserSessionHttpResponse? assumeRoleCampusUserSession)
        {
            UserId = userId;
            Username = username;
            Email = email;
            Fullname = fullname;
            PhotoURL = photoURL;
            AssumeRoleCampusUserWorkProfiles = assumeRoleCampusUserWorkProfiles;
            AssumeRoleCampusUserRoleCampuses = assumeRoleCampusUserRoleCampuses;
            AssumeRoleCampusUserSession = assumeRoleCampusUserSession;
        }
    }
}