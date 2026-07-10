using ESAM.GrowTracking.Domain.Enums;

namespace ESAM.GrowTracking.Application.Features.Auth.ChangeRoleCampus.Responses
{
    public record ChangeRoleCampusUserWorkProfileResponse
    {
        public int WorkProfileId { get; init; }

        public string WorkProfile { get; init; }

        public WorkProfileType WorkProfileType { get; init; }

        public ChangeRoleCampusUserWorkProfileResponse(int workProfileId, string workProfile, WorkProfileType workProfileType)
        {
            WorkProfileId = workProfileId;
            WorkProfile = workProfile;
            WorkProfileType = workProfileType;
        }
    }
}