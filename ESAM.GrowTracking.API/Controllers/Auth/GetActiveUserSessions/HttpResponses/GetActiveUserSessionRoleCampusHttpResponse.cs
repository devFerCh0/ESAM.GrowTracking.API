namespace ESAM.GrowTracking.API.Controllers.Auth.GetActiveUserSessions.HttpResponses
{
    public record GetActiveUserSessionRoleCampusHttpResponse
    {
        public int RoleId { get; init; }

        public string Role { get; init; }

        public int CampusId { get; init; }

        public string Campus { get; init; }

        public GetActiveUserSessionRoleCampusHttpResponse(int roleId, string role, int campusId, string campus)
        {
            RoleId = roleId;
            Role = role;
            CampusId = campusId;
            Campus = campus;
        }
    }
}
