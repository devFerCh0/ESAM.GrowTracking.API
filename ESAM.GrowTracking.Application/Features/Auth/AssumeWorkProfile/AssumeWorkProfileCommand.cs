using ESAM.GrowTracking.Application.Features.Auth.AssumeWorkProfile.Responses;
using ESAM.GrowTracking.Application.Results;
using MediatR;

namespace ESAM.GrowTracking.Application.Features.Auth.AssumeWorkProfile
{
    public record AssumeWorkProfileCommand : IRequest<Result<AssumeWorkProfileResponse>>
    {
        public int? WorkProfileId { get; init; }

        public AssumeWorkProfileCommand(int? workProfileId)
        {
            WorkProfileId = workProfileId;
        }
    }
}