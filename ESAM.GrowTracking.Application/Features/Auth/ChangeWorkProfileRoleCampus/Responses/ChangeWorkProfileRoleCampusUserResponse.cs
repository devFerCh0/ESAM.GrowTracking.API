namespace ESAM.GrowTracking.Application.Features.Auth.ChangeWorkProfileRoleCampus.Responses
{
    public record ChangeWorkProfileRoleCampusUserResponse
    {
        public int UserId { get; init; }

        public string Username { get; init; }

        public string Email { get; init; }

        public string Fullname { get; init; }

        public string? PhotoURL { get; init; }

        public List<ChangeWorkProfileRoleCampusUserWorkProfileResponse> ChangeWorkProfileRoleCampusUserWorkProfiles { get; init; }

        public List<ChangeWorkProfileRoleCampusUserRoleCampusResponse> ChangeWorkProfileRoleCampusUserRoleCampuses { get; init; }

        public ChangeWorkProfileRoleCampusUserSessionResponse? ChangeWorkProfileRoleCampusUserSession { get; init; }

        public ChangeWorkProfileRoleCampusUserResponse(int userId, string username, string email, string fullname, string? photoURL, 
            List<ChangeWorkProfileRoleCampusUserWorkProfileResponse> changeWorkProfileRoleCampusUserWorkProfiles, 
            List<ChangeWorkProfileRoleCampusUserRoleCampusResponse> changeWorkProfileRoleCampusUserRoleCampuses, 
            ChangeWorkProfileRoleCampusUserSessionResponse? changeWorkProfileRoleCampusUserSession)
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