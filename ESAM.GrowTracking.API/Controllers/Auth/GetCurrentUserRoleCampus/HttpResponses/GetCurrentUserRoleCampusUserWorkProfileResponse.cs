namespace ESAM.GrowTracking.API.Controllers.Auth.GetCurrentUserRoleCampus.HttpResponses
{
    public record GetCurrentUserRoleCampusUserWorkProfileHttpResponse
    {
        public int WorkProfileId { get; init; }

        public string WorkProfile { get; init; }

        public string WorkProfileType { get; init; }

        public GetCurrentUserRoleCampusUserWorkProfileHttpResponse(int workProfileId, string workProfile, string workProfileType)
        {
            WorkProfileId = workProfileId;
            WorkProfile = workProfile;
            WorkProfileType = workProfileType;
        }
    }
}