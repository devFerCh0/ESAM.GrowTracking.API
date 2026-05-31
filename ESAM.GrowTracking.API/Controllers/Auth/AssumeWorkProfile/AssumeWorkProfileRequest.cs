namespace ESAM.GrowTracking.API.Controllers.Auth.AssumeWorkProfile
{
    public record AssumeWorkProfileRequest
    {
        public int? WorkProfileId { get; init; }

        public AssumeWorkProfileRequest(int? workProfileId)
        {
            WorkProfileId = workProfileId;
        }
    }
}