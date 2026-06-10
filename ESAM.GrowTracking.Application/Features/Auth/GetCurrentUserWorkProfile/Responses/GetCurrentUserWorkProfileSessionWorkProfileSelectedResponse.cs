namespace ESAM.GrowTracking.Application.Features.Auth.GetCurrentUserWorkProfile.Responses
{
    public record GetCurrentUserWorkProfileSessionWorkProfileSelectedResponse
    {
        public int WorkProfileIdSelected { get; init; }

        public GetCurrentUserWorkProfileSessionWorkProfileSelectedResponse(int workProfileIdSelected)
        {
            WorkProfileIdSelected = workProfileIdSelected;
        }
    }
}