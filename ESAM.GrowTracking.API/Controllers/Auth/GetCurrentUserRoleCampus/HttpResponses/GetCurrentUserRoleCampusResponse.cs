namespace ESAM.GrowTracking.API.Controllers.Auth.GetCurrentUserRoleCampus.HttpResponses
{
    public record GetCurrentUserRoleCampusHttpResponse
    {
        public int UserId { get; init; }

        public string Username { get; init; }

        public string Email { get; init; }

        public string Fullname { get; init; }

        public string? PhotoURL { get; init; }

        public List<GetCurrentUserRoleCampusUserWorkProfileHttpResponse> CurrentUserRoleCampusUserWorkProfiles { get; init; }

        public List<GetCurrentUserRoleCampusUserRoleCampusHttpResponse> CurrentUserRoleCampusUserRoleCampuses { get; init; }

        public GetCurrentUserRoleCampusUserSessionHttpResponse CurrentUserRoleCampusUserSession { get; init; }

        public GetCurrentUserRoleCampusHttpResponse(int userId, string username, string email, string fullname, string? photoURL,
            List<GetCurrentUserRoleCampusUserWorkProfileHttpResponse> currentUserRoleCampusUserWorkProfiles, 
            List<GetCurrentUserRoleCampusUserRoleCampusHttpResponse> currentUserRoleCampusUserRoleCampuses,
            GetCurrentUserRoleCampusUserSessionHttpResponse currentUserRoleCampusUserSession)
        {
            UserId = userId;
            Username = username;
            Email = email;
            Fullname = fullname;
            PhotoURL = photoURL;
            CurrentUserRoleCampusUserWorkProfiles = currentUserRoleCampusUserWorkProfiles;
            CurrentUserRoleCampusUserRoleCampuses = currentUserRoleCampusUserRoleCampuses;
            CurrentUserRoleCampusUserSession = currentUserRoleCampusUserSession;
        }
    }
}