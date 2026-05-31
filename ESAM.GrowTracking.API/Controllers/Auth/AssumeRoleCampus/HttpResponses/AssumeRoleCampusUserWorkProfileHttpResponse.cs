namespace ESAM.GrowTracking.API.Controllers.Auth.AssumeRoleCampus.HttpResponses
{
    public record AssumeRoleCampusUserWorkProfileHttpResponse
    {
        public int WorkProfileId { get; init; }

        public string WorkProfile { get; init; }

        public string WorkProfileType { get; init; }

        public AssumeRoleCampusUserWorkProfileHttpResponse(int workProfileId, string workProfile, string workProfileType)
        {
            WorkProfileId = workProfileId;
            WorkProfile = workProfile;
            WorkProfileType = workProfileType;
        }
    }
}