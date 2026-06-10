namespace ESAM.GrowTracking.API.Controllers.Auth.GetCurrentUserRoleCampus.HttpResponses
{
    public record GetCurrentUserRoleCampusSessionRoleCampusSelectedHttpResponse
    {
        public int RoleIdSelected { get; init; }

        public int CampusIdSelected { get; init; }

        public GetCurrentUserRoleCampusSessionRoleCampusSelectedHttpResponse(int roleIdSelected, int campusIdSelected)
        {
            RoleIdSelected = roleIdSelected;
            CampusIdSelected = campusIdSelected;
        }
    }
}