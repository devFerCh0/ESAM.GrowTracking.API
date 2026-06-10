namespace ESAM.GrowTracking.API.Controllers.Auth.GetCurrentUserWorkProfile.HttpResponses
{
    public record GetCurrentUserWorkProfileUserWorkProfileHttpResponse
    {
        public int WorkProfileId { get; init; }

        public string WorkProfile { get; init; }

        public string WorkProfileType { get; init; }

        public GetCurrentUserWorkProfileUserWorkProfileHttpResponse(int workProfileId, string workProfile, string workProfileType)
        {
            WorkProfileId = workProfileId;
            WorkProfile = workProfile;
            WorkProfileType = workProfileType;
        }
    }
}