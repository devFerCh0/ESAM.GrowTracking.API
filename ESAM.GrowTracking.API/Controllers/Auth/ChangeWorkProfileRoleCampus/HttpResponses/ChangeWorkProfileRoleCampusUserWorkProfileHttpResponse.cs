namespace ESAM.GrowTracking.API.Controllers.Auth.ChangeWorkProfileRoleCampus.HttpResponses
{
    public record ChangeWorkProfileRoleCampusUserWorkProfileHttpResponse
    {
        public int WorkProfileId { get; init; }

        public string WorkProfile { get; init; }

        public string WorkProfileType { get; init; }

        public ChangeWorkProfileRoleCampusUserWorkProfileHttpResponse(int workProfileId, string workProfile, string workProfileType)
        {
            WorkProfileId = workProfileId;
            WorkProfile = workProfile;
            WorkProfileType = workProfileType;
        }
    }
}