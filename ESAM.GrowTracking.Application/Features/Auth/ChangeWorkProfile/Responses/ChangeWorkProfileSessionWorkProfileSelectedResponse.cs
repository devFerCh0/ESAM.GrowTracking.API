namespace ESAM.GrowTracking.Application.Features.Auth.ChangeWorkProfile.Responses
{
    public record ChangeWorkProfileSessionWorkProfileSelectedResponse
    {
        public int WorkProfileIdSelected { get; init; }

        public ChangeWorkProfileSessionWorkProfileSelectedResponse(int workProfileIdSelected)
        {
            WorkProfileIdSelected = workProfileIdSelected;
        }
    }
}