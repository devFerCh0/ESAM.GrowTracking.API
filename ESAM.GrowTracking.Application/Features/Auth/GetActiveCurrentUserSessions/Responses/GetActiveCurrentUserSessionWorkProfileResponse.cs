using ESAM.GrowTracking.Domain.Enums;

namespace ESAM.GrowTracking.Application.Features.Auth.GetActiveCurrentUserSessions.Responses
{
    public record GetActiveCurrentUserSessionWorkProfileResponse
    {
        public int WorkProfileId { get; init; }

        public string WorkProfile { get; init; }

        public WorkProfileType WorkProfileType { get; init; }

        public GetActiveCurrentUserSessionRoleCampusResponse? GetActiveCurrentUserSessionRoleCampus { get; init; }

        public GetActiveCurrentUserSessionWorkProfileResponse(int workProfileId, string workProfile, WorkProfileType workProfileType,
            GetActiveCurrentUserSessionRoleCampusResponse? getActiveCurrentUserSessionRoleCampus)
        {
            WorkProfileId = workProfileId;
            WorkProfile = workProfile;
            WorkProfileType = workProfileType;
            GetActiveCurrentUserSessionRoleCampus = getActiveCurrentUserSessionRoleCampus;
        }
    }
}