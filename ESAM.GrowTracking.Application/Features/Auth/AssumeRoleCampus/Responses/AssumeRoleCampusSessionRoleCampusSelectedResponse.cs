namespace ESAM.GrowTracking.Application.Features.Auth.AssumeRoleCampus.Responses
{
    public record AssumeRoleCampusSessionRoleCampusSelectedResponse
    {
        public int RoleIdSelected { get; init; }

        public int CampusIdSelected { get; init; }

        public AssumeRoleCampusSessionRoleCampusSelectedResponse(int roleIdSelected, int campusIdSelected)
        {
            RoleIdSelected = roleIdSelected;
            CampusIdSelected = campusIdSelected;
        }
    }
}