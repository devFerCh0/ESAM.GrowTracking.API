using ESAM.GrowTracking.Domain.Enums;

namespace ESAM.GrowTracking.Application.Features.Auth.GetCurrentUserRoleCampus.Responses
{
    public record GetCurrentUserRoleCampusUserWorkProfileResponse
    {
        public int WorkProfileId { get; init; }

        public string WorkProfile { get; init; }

        public WorkProfileType WorkProfileType { get; init; }

        public GetCurrentUserRoleCampusUserWorkProfileResponse(int workProfileId, string workProfile, WorkProfileType workProfileType)
        {
            WorkProfileId = workProfileId;
            WorkProfile = workProfile;
            WorkProfileType = workProfileType;
        }
    }
}