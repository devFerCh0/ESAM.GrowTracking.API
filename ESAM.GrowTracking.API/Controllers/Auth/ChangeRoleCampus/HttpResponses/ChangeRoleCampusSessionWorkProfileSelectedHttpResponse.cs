namespace ESAM.GrowTracking.API.Controllers.Auth.ChangeRoleCampus.HttpResponses
{
    public record ChangeRoleCampusSessionWorkProfileSelectedHttpResponse
    {
        public int WorkProfileIdSelected { get; init; }

        public ChangeRoleCampusSessionRoleCampusSelectedHttpResponse? ChangeRoleCampusSessionRoleCampusSelected { get; init; }

        public ChangeRoleCampusSessionWorkProfileSelectedHttpResponse(int workProfileIdSelected,
            ChangeRoleCampusSessionRoleCampusSelectedHttpResponse? changeRoleCampusSessionRoleCampusSelected)
        {
            WorkProfileIdSelected = workProfileIdSelected;
            ChangeRoleCampusSessionRoleCampusSelected = changeRoleCampusSessionRoleCampusSelected;
        }
    }
}