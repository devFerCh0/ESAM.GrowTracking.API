namespace ESAM.GrowTracking.API.Controllers.Auth.ChangeWorkProfile
{
    public record ChangeWorkProfileRequest
    {
        public int? WorkProfileId { get; init; }

        public ChangeWorkProfileRequest(int? workProfileId)
        {
            WorkProfileId = workProfileId;
        }
    }
}