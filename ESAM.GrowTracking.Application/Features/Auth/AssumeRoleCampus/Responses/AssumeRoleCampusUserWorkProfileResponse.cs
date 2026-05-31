using ESAM.GrowTracking.Domain.Enums;

namespace ESAM.GrowTracking.Application.Features.Auth.AssumeRoleCampus.Responses
{
    public record AssumeRoleCampusUserWorkProfileResponse
    {
        public int WorkProfileId { get; init; }

        public string WorkProfile { get; init; }

        public WorkProfileType WorkProfileType { get; init; }

        public AssumeRoleCampusUserWorkProfileResponse(int workProfileId, string workProfile, WorkProfileType workProfileType)
        {
            WorkProfileId = workProfileId;
            WorkProfile = workProfile;
            WorkProfileType = workProfileType;
        }
    }
}