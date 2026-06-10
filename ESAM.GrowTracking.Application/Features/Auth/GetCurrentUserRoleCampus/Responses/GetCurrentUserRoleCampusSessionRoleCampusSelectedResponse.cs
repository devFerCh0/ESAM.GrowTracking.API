namespace ESAM.GrowTracking.Application.Features.Auth.GetCurrentUserRoleCampus.Responses
{
    public record GetCurrentUserRoleCampusSessionRoleCampusSelectedResponse
    {
        public int RoleIdSelected { get; init; }

        public int CampusIdSelected { get; init; }

        public GetCurrentUserRoleCampusSessionRoleCampusSelectedResponse(int roleIdSelected, int campusIdSelected)
        {
            RoleIdSelected = roleIdSelected;
            CampusIdSelected = campusIdSelected;
        }
    }
}