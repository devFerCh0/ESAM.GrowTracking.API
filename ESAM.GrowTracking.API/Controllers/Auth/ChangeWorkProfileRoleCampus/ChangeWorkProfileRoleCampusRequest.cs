namespace ESAM.GrowTracking.API.Controllers.Auth.ChangeWorkProfileRoleCampus
{
    public record ChangeWorkProfileRoleCampusRequest
    {
        public int? WorkProfileId { get; init; }

        public int? RoleId { get; init; }

        public int? CampusId { get; init; }

        public ChangeWorkProfileRoleCampusRequest(int? workProfileId, int? roleId, int? campusId)
        {
            WorkProfileId = workProfileId;
            RoleId = roleId;
            CampusId = campusId;
        }
    }
}