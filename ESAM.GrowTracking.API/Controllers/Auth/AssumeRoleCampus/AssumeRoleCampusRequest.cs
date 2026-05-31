namespace ESAM.GrowTracking.API.Controllers.Auth.AssumeRoleCampus
{
    public record AssumeRoleCampusRequest
    {
        public int? WorkProfileId { get; init; }

        public int? RoleId { get; init; }

        public int? CampusId { get; init; }

        public AssumeRoleCampusRequest(int? workProfileId, int? roleId, int? campusId)
        {
            WorkProfileId = workProfileId;
            RoleId = roleId;
            CampusId = campusId;
        }
    }
}