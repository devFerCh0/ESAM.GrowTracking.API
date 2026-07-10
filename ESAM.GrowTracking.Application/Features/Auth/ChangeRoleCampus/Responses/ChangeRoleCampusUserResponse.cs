namespace ESAM.GrowTracking.Application.Features.Auth.ChangeRoleCampus.Responses
{
    public record ChangeRoleCampusUserResponse
    {
        public int UserId { get; init; }

        public string Username { get; init; }

        public string Email { get; init; }

        public string Fullname { get; init; }

        public string? PhotoURL { get; init; }

        public List<ChangeRoleCampusUserWorkProfileResponse> ChangeRoleCampusUserWorkProfiles { get; init; }

        public List<ChangeRoleCampusUserRoleCampusResponse> ChangeRoleCampusUserRoleCampuses { get; init; }

        public ChangeRoleCampusUserSessionResponse? ChangeRoleCampusUserSession { get; init; }

        public ChangeRoleCampusUserResponse(int userId, string username, string email, string fullname, string? photoURL,
            List<ChangeRoleCampusUserWorkProfileResponse> changeRoleCampusUserWorkProfiles, List<ChangeRoleCampusUserRoleCampusResponse> changeRoleCampusUserRoleCampuses,
            ChangeRoleCampusUserSessionResponse? changeRoleCampusUserSession)
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