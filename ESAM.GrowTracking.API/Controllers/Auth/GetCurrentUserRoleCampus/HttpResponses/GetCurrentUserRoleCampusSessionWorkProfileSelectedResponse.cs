namespace ESAM.GrowTracking.API.Controllers.Auth.GetCurrentUserRoleCampus.HttpResponses
{
    public record GetCurrentUserRoleCampusSessionWorkProfileSelectedHttpResponse
    {
        public int WorkProfileIdSelected { get; init; }

        public GetCurrentUserRoleCampusSessionRoleCampusSelectedHttpResponse? CurrentUserRoleCampusSessionRoleCampusSelected { get; init; }

        public GetCurrentUserRoleCampusSessionWorkProfileSelectedHttpResponse(int workProfileIdSelected,
            GetCurrentUserRoleCampusSessionRoleCampusSelectedHttpResponse? currentUserRoleCampusSessionRoleCampusSelected)
        {
            WorkProfileIdSelected = workProfileIdSelected;
            CurrentUserRoleCampusSessionRoleCampusSelected = currentUserRoleCampusSessionRoleCampusSelected;
        }
    }
}