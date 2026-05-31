namespace ESAM.GrowTracking.API.Controllers.Auth.Login.HttpResponses
{
    public record LoginUserWorkProfileHttpResponse
    {
        public int WorkProfileId { get; init; }

        public string WorkProfile { get; init; }

        public string WorkProfileType { get; init; }

        public LoginUserWorkProfileHttpResponse(int workProfileId, string workProfile, string workProfileType)
        {
            WorkProfileId = workProfileId;
            WorkProfile = workProfile;
            WorkProfileType = workProfileType;
        }
    }
}