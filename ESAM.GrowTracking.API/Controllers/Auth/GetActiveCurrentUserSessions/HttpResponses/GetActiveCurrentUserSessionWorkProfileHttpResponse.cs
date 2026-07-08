using ESAM.GrowTracking.Domain.Enums;

namespace ESAM.GrowTracking.API.Controllers.Auth.GetActiveCurrentUserSessions.HttpResponses
{
    public record GetActiveCurrentUserSessionWorkProfileHttpResponse
    {
        public int WorkProfileId { get; init; }

        public string WorkProfile { get; init; }

        public WorkProfileType WorkProfileType { get; init; }

        public GetActiveCurrentUserSessionRoleCampusHttpResponse? GetActiveCurrentUserSessionRoleCampus { get; init; }

        public GetActiveCurrentUserSessionWorkProfileHttpResponse(int workProfileId, string workProfile, WorkProfileType workProfileType,
            GetActiveCurrentUserSessionRoleCampusHttpResponse? getActiveCurrentUserSessionRoleCampus)
        {
            WorkProfileId = workProfileId;
            WorkProfile = workProfile;
            WorkProfileType = workProfileType;
            GetActiveCurrentUserSessionRoleCampus = getActiveCurrentUserSessionRoleCampus;
        }
    }
}