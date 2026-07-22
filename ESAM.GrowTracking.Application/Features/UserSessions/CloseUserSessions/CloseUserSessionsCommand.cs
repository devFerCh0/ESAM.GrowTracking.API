using ESAM.GrowTracking.Application.Results;
using MediatR;

namespace ESAM.GrowTracking.Application.Features.UserSessions.CloseUserSessions
{
    public record CloseUserSessionsCommand : IRequest<Result>
    {
        public int UserId { get; init; }

        public CloseUserSessionsCommand(int userId)
        {
            UserId = userId;
        }
    }
}