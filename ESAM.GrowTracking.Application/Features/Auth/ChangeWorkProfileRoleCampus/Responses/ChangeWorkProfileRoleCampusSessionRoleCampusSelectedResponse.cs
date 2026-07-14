namespace ESAM.GrowTracking.Application.Features.Auth.ChangeWorkProfileRoleCampus.Responses
{
    public record ChangeWorkProfileRoleCampusSessionRoleCampusSelectedResponse
    {
        public int RoleIdSelected { get; init; }

        public int CampusIdSelected { get; init; }

        public ChangeWorkProfileRoleCampusSessionRoleCampusSelectedResponse(int roleIdSelected, int campusIdSelected)
        {
            RoleIdSelected = roleIdSelected;
            CampusIdSelected = campusIdSelected;
        }
    }
}