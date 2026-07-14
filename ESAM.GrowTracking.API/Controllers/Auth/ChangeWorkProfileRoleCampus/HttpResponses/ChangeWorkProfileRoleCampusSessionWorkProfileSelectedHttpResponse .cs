namespace ESAM.GrowTracking.API.Controllers.Auth.ChangeWorkProfileRoleCampus.HttpResponses
{
    public record ChangeWorkProfileRoleCampusSessionWorkProfileSelectedHttpResponse
    {
        public int WorkProfileIdSelected { get; init; }

        public ChangeWorkProfileRoleCampusSessionRoleCampusSelectedHttpResponse ChangeWorkProfileRoleCampusSessionRoleCampusSelected { get; init; }

        public ChangeWorkProfileRoleCampusSessionWorkProfileSelectedHttpResponse(int workProfileIdSelected,
            ChangeWorkProfileRoleCampusSessionRoleCampusSelectedHttpResponse changeWorkProfileRoleCampusSessionRoleCampusSelected)
        {
            WorkProfileIdSelected = workProfileIdSelected;
            ChangeWorkProfileRoleCampusSessionRoleCampusSelected = changeWorkProfileRoleCampusSessionRoleCampusSelected;
        }
    }
}