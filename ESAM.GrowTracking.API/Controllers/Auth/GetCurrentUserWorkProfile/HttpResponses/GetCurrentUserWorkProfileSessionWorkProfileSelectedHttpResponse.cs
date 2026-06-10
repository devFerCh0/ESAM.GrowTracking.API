namespace ESAM.GrowTracking.API.Controllers.Auth.GetCurrentUserWorkProfile.HttpResponses
{
    public record GetCurrentUserWorkProfileSessionWorkProfileSelectedHttpResponse
    {
        public int WorkProfileIdSelected { get; init; }

        public GetCurrentUserWorkProfileSessionWorkProfileSelectedHttpResponse(int workProfileIdSelected)
        {
            WorkProfileIdSelected = workProfileIdSelected;
        }
    }
}