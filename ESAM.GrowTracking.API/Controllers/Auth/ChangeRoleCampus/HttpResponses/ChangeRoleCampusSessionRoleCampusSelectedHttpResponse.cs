namespace ESAM.GrowTracking.API.Controllers.Auth.ChangeRoleCampus.HttpResponses
{
    public record ChangeRoleCampusSessionRoleCampusSelectedHttpResponse
    {
        public int RoleIdSelected { get; init; }

        public int CampusIdSelected { get; init; }

        public ChangeRoleCampusSessionRoleCampusSelectedHttpResponse(int roleIdSelected, int campusIdSelected)
        {
            RoleIdSelected = roleIdSelected;
            CampusIdSelected = campusIdSelected;
        }
    }
}