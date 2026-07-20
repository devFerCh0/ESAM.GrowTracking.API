using ESAM.GrowTracking.Domain.Enums;

namespace ESAM.GrowTracking.Application.Features.Users.GetActiveUserSessions.Responses
{
    public record GetActiveUserSessionWorkProfileResponse
    {
        public int WorkProfileId { get; init; }

        public string WorkProfile { get; init; }

        public WorkProfileType WorkProfileType { get; init; }

        public GetActiveUserSessionRoleCampusResponse? GetActiveUserSessionRoleCampus { get; init; }

        public GetActiveUserSessionWorkProfileResponse(int workProfileId, string workProfile, WorkProfileType workProfileType, 
            GetActiveUserSessionRoleCampusResponse? getActiveUserSessionRoleCampus)
        {
            WorkProfileId = workProfileId;
            WorkProfile = workProfile;
            WorkProfileType = workProfileType;
            GetActiveUserSessionRoleCampus = getActiveUserSessionRoleCampus;
        }
    }
}