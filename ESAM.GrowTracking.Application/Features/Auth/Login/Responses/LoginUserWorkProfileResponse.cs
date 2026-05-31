using ESAM.GrowTracking.Domain.Enums;

namespace ESAM.GrowTracking.Application.Features.Auth.Login.Responses
{
    public record LoginUserWorkProfileResponse
    {
        public int WorkProfileId { get; init; }

        public string WorkProfile { get; init; }

        public WorkProfileType WorkProfileType { get; init; }

        public LoginUserWorkProfileResponse(int workProfileId, string workProfile, WorkProfileType workProfileType)
        {
            WorkProfileId = workProfileId;
            WorkProfile = workProfile;
            WorkProfileType = workProfileType;
        }
    }
}