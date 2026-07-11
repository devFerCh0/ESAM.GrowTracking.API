namespace ESAM.GrowTracking.API.Controllers.Auth.ChangeRoleCampus.HttpResponses
{
    public record ChangeRoleCampusUserWorkProfileHttpResponse
    {
        public int WorkProfileId { get; init; }

        public string WorkProfile { get; init; }

        public string WorkProfileType { get; init; }

        public ChangeRoleCampusUserWorkProfileHttpResponse(int workProfileId, string workProfile, string workProfileType)
        {
            WorkProfileId = workProfileId;
            WorkProfile = workProfile;
            WorkProfileType = workProfileType;
        }
    }
}