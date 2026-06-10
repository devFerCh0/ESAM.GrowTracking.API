using ESAM.GrowTracking.Application.Features.Auth.GetCurrentUserRoleCampus.Responses;
using ESAM.GrowTracking.Application.Results;
using MediatR;

namespace ESAM.GrowTracking.Application.Features.Auth.GetCurrentUserRoleCampus
{
    public record GetCurrentUserRoleCampusQuery : IRequest<Result<GetCurrentUserRoleCampusResponse>>
    {
        public GetCurrentUserRoleCampusQuery() { }
    }
}