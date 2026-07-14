using ESAM.GrowTracking.Application.Results;
using MediatR;

namespace ESAM.GrowTracking.Application.Features.Auth.GetChangeUserRoleCampuses
{
    public record GetChangeUserRoleCampusesQuery : IRequest<Result<List<GetChangeUserRoleCampusResponse>>>
    {
        public int WorkProfileId { get; init; }

        public GetChangeUserRoleCampusesQuery(int workProfileId)
        {
            WorkProfileId = workProfileId;
        }
    }
}