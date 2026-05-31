namespace ESAM.GrowTracking.Application.Features.Auth.AssumeWorkProfile.Responses
{
    public record AssumeWorkProfileSessionWorkProfileSelectedResponse
    {
        public int WorkProfileIdSelected { get; init; }

        public AssumeWorkProfileSessionWorkProfileSelectedResponse(int workProfileIdSelected)
        {
            WorkProfileIdSelected = workProfileIdSelected;
        }
    }
}