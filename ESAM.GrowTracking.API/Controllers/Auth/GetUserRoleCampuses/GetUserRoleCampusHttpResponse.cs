namespace ESAM.GrowTracking.API.Controllers.Auth.GetUserRoleCampuses
{
    public record GetUserRoleCampusHttpResponse
    {
        public int RoleId { get; init; }

        public string Role { get; init; }

        public int CampusId { get; init; }

        public string Campus { get; init; }

        public GetUserRoleCampusHttpResponse(int roleId, string role, int campusId, string campus)
        {
            RoleId = roleId;
            Role = role;
            CampusId = campusId;
            Campus = campus;
        }
    }
}