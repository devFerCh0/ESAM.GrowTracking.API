namespace ESAM.GrowTracking.API.Controllers.Auth.ChangeRoleCampus
{
    public record ChangeRoleCampusRequest
    {
        public int? RoleId { get; init; }

        public int? CampusId { get; init; }

        public ChangeRoleCampusRequest(int? roleId, int? campusId)
        {
            RoleId = roleId;
            CampusId = campusId;
        }
    }
}