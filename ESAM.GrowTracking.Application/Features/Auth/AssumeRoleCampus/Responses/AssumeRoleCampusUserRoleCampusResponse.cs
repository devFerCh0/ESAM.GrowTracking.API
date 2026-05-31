namespace ESAM.GrowTracking.Application.Features.Auth.AssumeRoleCampus.Responses
{
    public record AssumeRoleCampusUserRoleCampusResponse
    {
        public int RoleId { get; init; }

        public string Role { get; init; }

        public int CampusId { get; init; }

        public string Campus { get; init; }

        public AssumeRoleCampusUserRoleCampusResponse(int roleId, string role, int campusId, string campus)
        {
            RoleId = roleId;
            Role = role;
            CampusId = campusId;
            Campus = campus;
        }
    }
}