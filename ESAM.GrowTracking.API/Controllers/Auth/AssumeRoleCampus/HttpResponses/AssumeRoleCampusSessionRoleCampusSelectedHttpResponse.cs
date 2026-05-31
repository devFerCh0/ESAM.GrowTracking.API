namespace ESAM.GrowTracking.API.Controllers.Auth.AssumeRoleCampus.HttpResponses
{
    public record AssumeRoleCampusSessionRoleCampusSelectedHttpResponse
    {
        public int RoleIdSelected { get; init; }

        public int CampusIdSelected { get; init; }

        public AssumeRoleCampusSessionRoleCampusSelectedHttpResponse(int roleIdSelected, int campusIdSelected)
        {
            RoleIdSelected = roleIdSelected;
            CampusIdSelected = campusIdSelected;
        }
    }
}