namespace ESAM.GrowTracking.API.Controllers.Users.GetUsers.HttpResponses
{
    public record GetUsersUserWorkProfileHttpResponse
    {
        public int WorkProfileId { get; init; }

        public string WorkProfile { get; init; }

        public string WorkProfileType { get; init; }

        public GetUsersUserWorkProfileHttpResponse(int workProfileId, string workProfile, string workProfileType)
        {
            WorkProfileId = workProfileId;
            WorkProfile = workProfile;
            WorkProfileType = workProfileType;
        }
    }
}