namespace ESAM.GrowTracking.API.Controllers.Auth.ChangeWorkProfile.HttpResponses
{
    public record ChangeWorkProfileSessionWorkProfileSelectedHttpResponse
    {
        public int WorkProfileIdSelected { get; init; }

        public ChangeWorkProfileSessionWorkProfileSelectedHttpResponse(int workProfileIdSelected)
        {
            WorkProfileIdSelected = workProfileIdSelected;
        }
    }
}