namespace ESAM.GrowTracking.API.Controllers.Auth.ChangeWorkProfile.HttpResponses
{
    public record ChangeWorkProfileUserWorkProfileHttpResponse
    {
        public int WorkProfileId { get; init; }

        public string WorkProfile { get; init; }

        public string WorkProfileType { get; init; }

        public ChangeWorkProfileUserWorkProfileHttpResponse(int workProfileId, string workProfile, string workProfileType)
        {
            WorkProfileId = workProfileId;
            WorkProfile = workProfile;
            WorkProfileType = workProfileType;
        }
    }
}