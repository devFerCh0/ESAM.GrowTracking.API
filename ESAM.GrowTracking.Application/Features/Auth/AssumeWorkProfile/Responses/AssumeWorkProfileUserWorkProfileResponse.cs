using ESAM.GrowTracking.Domain.Enums;

namespace ESAM.GrowTracking.Application.Features.Auth.AssumeWorkProfile.Responses
{
    public record AssumeWorkProfileUserWorkProfileResponse
    {
        public int WorkProfileId { get; init; }

        public string WorkProfile { get; init; }

        public WorkProfileType WorkProfileType { get; init; }

        public AssumeWorkProfileUserWorkProfileResponse(int workProfileId, string workProfile, WorkProfileType workProfileType)
        {
            WorkProfileId = workProfileId;
            WorkProfile = workProfile;
            WorkProfileType = workProfileType;
        }
    }
}