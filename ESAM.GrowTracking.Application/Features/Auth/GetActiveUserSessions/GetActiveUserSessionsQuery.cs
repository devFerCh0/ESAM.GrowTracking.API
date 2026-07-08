using ESAM.GrowTracking.Application.Features.Auth.GetActiveUserSessions.Responses;
using ESAM.GrowTracking.Application.Results;
using MediatR;

namespace ESAM.GrowTracking.Application.Features.Auth.GetActiveUserSessions
{
    public record GetActiveUserSessionsQuery : IRequest<Result<List<GetActiveUserSessionsResponse>>>
    {
        public int UserId { get; init; }

        public GetActiveUserSessionsQuery(int userId)
        {
            UserId = userId;
        }
    }
}