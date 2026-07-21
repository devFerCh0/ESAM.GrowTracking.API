using ESAM.GrowTracking.Domain.Enums;

namespace ESAM.GrowTracking.Application.Features.UserSessions.GetUserSessions.Responses
{
    public record GetUserSessionWorkProfileSelectedResponse
    {
        public int WorkProfileId { get; init; }

        public string WorkProfile { get; init; }

        public WorkProfileType WorkProfileType { get; init; }

        public GetUserSessionWorkProfileRoleCampusSelectedResponse? GetUserSessionWorkProfileRoleCampusSelected { get; init; }

        public GetUserSessionWorkProfileSelectedResponse(int workProfileId, string workProfile, WorkProfileType workProfileType,
            GetUserSessionWorkProfileRoleCampusSelectedResponse? getUserSessionWorkProfileRoleCampusSelected)
        {
            WorkProfileId = workProfileId;
            WorkProfile = workProfile;
            WorkProfileType = workProfileType;
            GetUserSessionWorkProfileRoleCampusSelected = getUserSessionWorkProfileRoleCampusSelected;
        }
    }
}