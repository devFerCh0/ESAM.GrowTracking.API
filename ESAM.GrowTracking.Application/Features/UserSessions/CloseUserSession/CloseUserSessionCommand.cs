using ESAM.GrowTracking.Application.Results;
using MediatR;

namespace ESAM.GrowTracking.Application.Features.UserSessions.CloseUserSession
{
    public record CloseUserSessionCommand : IRequest<Result>
    {
        public int UserSessionId { get; init; }

        public int UserId { get; init; }

        public CloseUserSessionCommand(int userSessionId, int userId)
        {
            UserSessionId = userSessionId;
            UserId = userId;
        }
    }
}