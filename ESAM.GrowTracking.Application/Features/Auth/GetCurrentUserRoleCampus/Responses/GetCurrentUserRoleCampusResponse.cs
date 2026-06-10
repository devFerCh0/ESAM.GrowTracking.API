namespace ESAM.GrowTracking.Application.Features.Auth.GetCurrentUserRoleCampus.Responses
{
    public record GetCurrentUserRoleCampusResponse
    {
        public int UserId { get; init; }

        public string Username { get; init; }

        public string Email { get; init; }

        public string Fullname { get; init; }

        public string? PhotoURL { get; init; }

        public List<GetCurrentUserRoleCampusUserWorkProfileResponse> CurrentUserRoleCampusUserWorkProfiles { get; init; }

        public List<GetCurrentUserRoleCampusUserRoleCampusResponse> CurrentUserRoleCampusUserRoleCampuses { get; init; }

        public GetCurrentUserRoleCampusUserSessionResponse? CurrentUserRoleCampusUserSession { get; init; }

        public GetCurrentUserRoleCampusResponse(int userId, string username, string email, string fullname, string? photoURL,
            List<GetCurrentUserRoleCampusUserWorkProfileResponse> currentUserRoleCampusUserWorkProfiles, 
            List<GetCurrentUserRoleCampusUserRoleCampusResponse> currentUserRoleCampusUserRoleCampuses,
            GetCurrentUserRoleCampusUserSessionResponse? currentUserRoleCampusUserSession)
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