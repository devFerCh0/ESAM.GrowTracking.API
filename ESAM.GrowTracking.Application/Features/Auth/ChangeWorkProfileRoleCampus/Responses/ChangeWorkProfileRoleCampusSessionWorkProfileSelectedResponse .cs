namespace ESAM.GrowTracking.Application.Features.Auth.ChangeWorkProfileRoleCampus.Responses
{
    public record ChangeWorkProfileRoleCampusSessionWorkProfileSelectedResponse
    {
        public int WorkProfileIdSelected { get; init; }

        public ChangeWorkProfileRoleCampusSessionRoleCampusSelectedResponse? ChangeWorkProfileRoleCampusSessionRoleCampusSelected { get; init; }

        public ChangeWorkProfileRoleCampusSessionWorkProfileSelectedResponse(int workProfileIdSelected,
            ChangeWorkProfileRoleCampusSessionRoleCampusSelectedResponse? changeWorkProfileRoleCampusSessionRoleCampusSelected)
        {
            WorkProfileIdSelected = workProfileIdSelected;
            ChangeWorkProfileRoleCampusSessionRoleCampusSelected = changeWorkProfileRoleCampusSessionRoleCampusSelected;
        }
    }
}