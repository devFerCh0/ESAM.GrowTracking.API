namespace ESAM.GrowTracking.Application.Features.Auth.ChangeWorkProfileRoleCampus.Responses
{
    public record ChangeWorkProfileRoleCampusUserRoleCampusResponse
    {
        public int RoleId { get; init; }

        public string Role { get; init; }

        public int CampusId { get; init; }

        public string Campus { get; init; }

        public ChangeWorkProfileRoleCampusUserRoleCampusResponse(int roleId, string role, int campusId, string campus)
        {
            RoleId = roleId;
            Role = role;
            CampusId = campusId;
            Campus = campus;
        }
    }
}