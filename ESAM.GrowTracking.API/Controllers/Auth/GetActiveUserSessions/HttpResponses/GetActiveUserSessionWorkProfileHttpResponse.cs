using ESAM.GrowTracking.Domain.Enums;

namespace ESAM.GrowTracking.API.Controllers.Auth.GetActiveUserSessions.HttpResponses
{
    public record GetActiveUserSessionWorkProfileHttpResponse
    {
        public int WorkProfileId { get; init; }

        public string WorkProfile { get; init; }

        public WorkProfileType WorkProfileType { get; init; }

        public GetActiveUserSessionRoleCampusHttpResponse? GetActiveUserSessionRoleCampus { get; init; }

        public GetActiveUserSessionWorkProfileHttpResponse(int workProfileId, string workProfile, WorkProfileType workProfileType,
            GetActiveUserSessionRoleCampusHttpResponse? getActiveUserSessionRoleCampus)
        {
            WorkProfileId = workProfileId;
            WorkProfile = workProfile;
            WorkProfileType = workProfileType;
            GetActiveUserSessionRoleCampus = getActiveUserSessionRoleCampus;
        }
    }
}