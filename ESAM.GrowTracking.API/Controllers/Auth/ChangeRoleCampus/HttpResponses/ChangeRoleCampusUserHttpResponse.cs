namespace ESAM.GrowTracking.API.Controllers.Auth.ChangeRoleCampus.HttpResponses
{
    public record ChangeRoleCampusUserHttpResponse
    {
        public int UserId { get; init; }

        public string Username { get; init; }

        public string Email { get; init; }

        public string Fullname { get; init; }

        public string? PhotoURL { get; init; }

        public List<ChangeRoleCampusUserWorkProfileHttpResponse> ChangeRoleCampusUserWorkProfiles { get; init; }

        public List<ChangeRoleCampusUserRoleCampusHttpResponse> ChangeRoleCampusUserRoleCampuses { get; init; }

        public ChangeRoleCampusUserSessionHttpResponse ChangeRoleCampusUserSession { get; init; }

        public ChangeRoleCampusUserHttpResponse(int userId, string username, string email, string fullname, string? photoURL,
            List<ChangeRoleCampusUserWorkProfileHttpResponse> changeRoleCampusUserWorkProfiles, List<ChangeRoleCampusUserRoleCampusHttpResponse> changeRoleCampusUserRoleCampuses,
            ChangeRoleCampusUserSessionHttpResponse changeRoleCampusUserSession)
        {
            UserId = userId;
            Username = username;
            Email = email;
            Fullname = fullname;
            PhotoURL = photoURL;
            ChangeRoleCampusUserWorkProfiles = changeRoleCampusUserWorkProfiles;
            ChangeRoleCampusUserRoleCampuses = changeRoleCampusUserRoleCampuses;
            ChangeRoleCampusUserSession = changeRoleCampusUserSession;
        }
    }
}