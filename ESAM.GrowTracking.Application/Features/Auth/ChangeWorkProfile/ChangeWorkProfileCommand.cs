using ESAM.GrowTracking.Application.Features.Auth.ChangeWorkProfile.Responses;
using ESAM.GrowTracking.Application.Results;
using MediatR;

namespace ESAM.GrowTracking.Application.Features.Auth.ChangeWorkProfile
{
    public record ChangeWorkProfileCommand : IRequest<Result<ChangeWorkProfileResponse>>
    {
        public int WorkProfileId { get; init; }

        public ChangeWorkProfileCommand(int workProfileId)
        {
            WorkProfileId = workProfileId;
        }
    }
}