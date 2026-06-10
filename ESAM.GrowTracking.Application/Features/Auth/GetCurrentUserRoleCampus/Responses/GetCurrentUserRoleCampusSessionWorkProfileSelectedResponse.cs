namespace ESAM.GrowTracking.Application.Features.Auth.GetCurrentUserRoleCampus.Responses
{
    public record GetCurrentUserRoleCampusSessionWorkProfileSelectedResponse
    {
        public int WorkProfileIdSelected { get; init; }

        public GetCurrentUserRoleCampusSessionRoleCampusSelectedResponse? CurrentUserRoleCampusSessionRoleCampusSelected { get; init; }

        public GetCurrentUserRoleCampusSessionWorkProfileSelectedResponse(int workProfileIdSelected,
            GetCurrentUserRoleCampusSessionRoleCampusSelectedResponse? currentUserRoleCampusSessionRoleCampusSelected)
        {
            WorkProfileIdSelected = workProfileIdSelected;
            CurrentUserRoleCampusSessionRoleCampusSelected = currentUserRoleCampusSessionRoleCampusSelected;
        }
    }
}