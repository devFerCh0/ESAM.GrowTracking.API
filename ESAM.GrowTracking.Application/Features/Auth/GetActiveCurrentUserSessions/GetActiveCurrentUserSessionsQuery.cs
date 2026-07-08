using ESAM.GrowTracking.Application.Features.Auth.GetActiveCurrentUserSessions.Responses;
using ESAM.GrowTracking.Application.Results;
using MediatR;

namespace ESAM.GrowTracking.Application.Features.Auth.GetActiveCurrentUserSessions
{
    public record GetActiveCurrentUserSessionsQuery : IRequest<Result<List<GetActiveCurrentUserSessionsResponse>>>
    {
        public GetActiveCurrentUserSessionsQuery() { }
    }
}