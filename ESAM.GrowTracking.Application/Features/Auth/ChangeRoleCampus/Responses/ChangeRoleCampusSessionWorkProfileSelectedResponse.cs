namespace ESAM.GrowTracking.Application.Features.Auth.ChangeRoleCampus.Responses
{
    public record ChangeRoleCampusSessionWorkProfileSelectedResponse
    {
        public int WorkProfileIdSelected { get; init; }

        public ChangeRoleCampusSessionRoleCampusSelectedResponse? ChangeRoleCampusSessionRoleCampusSelected { get; init; }

        public ChangeRoleCampusSessionWorkProfileSelectedResponse(int workProfileIdSelected,
            ChangeRoleCampusSessionRoleCampusSelectedResponse? changeRoleCampusSessionRoleCampusSelected)
        {
            WorkProfileIdSelected = workProfileIdSelected;
            ChangeRoleCampusSessionRoleCampusSelected = changeRoleCampusSessionRoleCampusSelected;
        }
    }
}