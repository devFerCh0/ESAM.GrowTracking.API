namespace ESAM.GrowTracking.API.Controllers.Auth.AssumeWorkProfile.HttpResponses
{
    public record AssumeWorkProfileSessionWorkProfileSelectedHttpResponse
    {
        public int WorkProfileIdSelected { get; init; }

        public AssumeWorkProfileSessionWorkProfileSelectedHttpResponse(int workProfileIdSelected)
        {
            WorkProfileIdSelected = workProfileIdSelected;
        }
    }
}