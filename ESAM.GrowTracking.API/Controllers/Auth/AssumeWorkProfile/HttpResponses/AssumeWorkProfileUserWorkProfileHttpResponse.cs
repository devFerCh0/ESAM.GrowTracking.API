namespace ESAM.GrowTracking.API.Controllers.Auth.AssumeWorkProfile.HttpResponses
{
    public record AssumeWorkProfileUserWorkProfileHttpResponse
    {
        public int WorkProfileId { get; init; }

        public string WorkProfile { get; init; }

        public string WorkProfileType { get; init; }

        public AssumeWorkProfileUserWorkProfileHttpResponse(int workProfileId, string workProfile, string workProfileType)
        {
            WorkProfileId = workProfileId;
            WorkProfile = workProfile;
            WorkProfileType = workProfileType;
        }
    }
}