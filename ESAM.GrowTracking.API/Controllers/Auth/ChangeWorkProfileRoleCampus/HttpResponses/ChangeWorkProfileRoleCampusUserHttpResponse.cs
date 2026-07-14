namespace ESAM.GrowTracking.API.Controllers.Auth.ChangeWorkProfileRoleCampus.HttpResponses
{
    public record ChangeWorkProfileRoleCampusUserHttpResponse
    {
        public int UserId { get; init; }

        public string Username { get; init; }

        public string Email { get; init; }

        public string Fullname { get; init; }

        public string? PhotoURL { get; init; }

        public List<ChangeWorkProfileRoleCampusUserWorkProfileHttpResponse> ChangeWorkProfileRoleCampusUserWorkProfiles { get; init; }

        public List<ChangeWorkProfileRoleCampusUserRoleCampusHttpResponse> ChangeWorkProfileRoleCampusUserRoleCampuses { get; init; }

        public ChangeWorkProfileRoleCampusUserSessionHttpResponse ChangeWorkProfileRoleCampusUserSession { get; init; }

        public ChangeWorkProfileRoleCampusUserHttpResponse(int userId, string username, string email, string fullname, string? photoURL, 
            List<ChangeWorkProfileRoleCampusUserWorkProfileHttpResponse> changeWorkProfileRoleCampusUserWorkProfiles, 
            List<ChangeWorkProfileRoleCampusUserRoleCampusHttpResponse> changeWorkProfileRoleCampusUserRoleCampuses,
            ChangeWorkProfileRoleCampusUserSessionHttpResponse changeWorkProfileRoleCampusUserSession)
        {
            UserId = userId;
            Username = username;
            Email = email;
            Fullname = fullname;
            PhotoURL = photoURL;
            ChangeWorkProfileRoleCampusUserWorkProfiles = changeWorkProfileRoleCampusUserWorkProfiles;
            ChangeWorkProfileRoleCampusUserRoleCampuses = changeWorkProfileRoleCampusUserRoleCampuses;
            ChangeWorkProfileRoleCampusUserSession = changeWorkProfileRoleCampusUserSession;
        }
    }
}