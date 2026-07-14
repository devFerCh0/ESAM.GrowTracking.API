namespace ESAM.GrowTracking.Application.Features.Auth.GetChangeUserRoleCampuses
{
    public record GetChangeUserRoleCampusResponse
    {
        public int RoleId { get; init; }

        public string Role { get; init; }

        public int CampusId { get; init; }

        public string Campus { get; init; }

        public GetChangeUserRoleCampusResponse(int roleId, string role, int campusId, string campus)
        {
            RoleId = roleId;
            Role = role;
            CampusId = campusId;
            Campus = campus;
        }
    }
}