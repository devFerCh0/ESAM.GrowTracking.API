namespace ESAM.GrowTracking.API.Controllers.Auth.AssumeRoleCampus.HttpResponses
{
    public record AssumeRoleCampusSessionWorkProfileSelectedHttpResponse
    {
        public int WorkProfileIdSelected { get; init; }

        public AssumeRoleCampusSessionRoleCampusSelectedHttpResponse AssumeRoleCampusSessionRoleCampusSelected { get; init; }

        public AssumeRoleCampusSessionWorkProfileSelectedHttpResponse(int workProfileIdSelected, 
            AssumeRoleCampusSessionRoleCampusSelectedHttpResponse assumeRoleCampusSessionRoleCampusSelected)
        {
            WorkProfileIdSelected = workProfileIdSelected;
            AssumeRoleCampusSessionRoleCampusSelected = assumeRoleCampusSessionRoleCampusSelected;
        }
    }
}