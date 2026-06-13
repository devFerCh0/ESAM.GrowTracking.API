using ESAM.GrowTracking.Application.Results;
using MediatR;

namespace ESAM.GrowTracking.Application.Features.Auth.GetUserRoleCampuses
{
    public record GetUserRoleCampusesQuery : IRequest<Result<List<GetUserRoleCampusResponse>>>
    {
        public int WorkProfileId { get; init; }

        public GetUserRoleCampusesQuery(int workProfileId)
        {
            WorkProfileId = workProfileId;
        }
    }
}