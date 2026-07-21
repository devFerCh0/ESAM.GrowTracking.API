namespace ESAM.GrowTracking.Application.Features.UserSessions.GetUserSessions.Responses
{
    public record GetUserSessionWorkProfileRoleCampusSelectedResponse
    {
        public int RoleId { get; init; }

        public string Role { get; init; }

        public int CampusId { get; init; }

        public string Campus { get; init; }

        public GetUserSessionWorkProfileRoleCampusSelectedResponse(int roleId, string role, int campusId, string campus)
        {
            RoleId = roleId;
            Role = role;
            CampusId = campusId;
            Campus = campus;
        }
    }
}