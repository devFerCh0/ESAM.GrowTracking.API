namespace ESAM.GrowTracking.Application.Features.Auth.AssumeRoleCampus.Responses
{
    public record AssumeRoleCampusSessionWorkProfileSelectedResponse
    {
        public int WorkProfileIdSelected { get; init; }

        public AssumeRoleCampusSessionRoleCampusSelectedResponse? AssumeRoleCampusSessionRoleCampusSelected { get; init; }

        public AssumeRoleCampusSessionWorkProfileSelectedResponse(int workProfileIdSelected, 
            AssumeRoleCampusSessionRoleCampusSelectedResponse? assumeRoleCampusSessionRoleCampusSelected)
        {
            WorkProfileIdSelected = workProfileIdSelected;
            AssumeRoleCampusSessionRoleCampusSelected = assumeRoleCampusSessionRoleCampusSelected;
        }
    }
}