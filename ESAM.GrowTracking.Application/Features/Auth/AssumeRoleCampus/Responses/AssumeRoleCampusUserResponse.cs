namespace ESAM.GrowTracking.Application.Features.Auth.AssumeRoleCampus.Responses
{
    public record AssumeRoleCampusUserResponse
    {
        public int UserId { get; init; }

        public string Username { get; init; }

        public string Email { get; init; }

        public string Fullname { get; init; }

        public string? PhotoURL { get; init; }

        public List<AssumeRoleCampusUserWorkProfileResponse> AssumeRoleCampusUserWorkProfiles { get; init; }

        public List<AssumeRoleCampusUserRoleCampusResponse> AssumeRoleCampusUserRoleCampuses { get; init; }

        public AssumeRoleCampusUserSessionResponse? AssumeRoleCampusUserSession { get; init; }

        public AssumeRoleCampusUserResponse(int userId, string username, string email, string fullname, string? photoURL, 
            List<AssumeRoleCampusUserWorkProfileResponse> assumeRoleCampusUserWorkProfiles, List<AssumeRoleCampusUserRoleCampusResponse> assumeRoleCampusUserRoleCampuses,
            AssumeRoleCampusUserSessionResponse? assumeRoleCampusUserSession)
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