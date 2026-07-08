using ESAM.GrowTracking.Application.Results;
using MediatR;

namespace ESAM.GrowTracking.Application.Features.Auth.RevokeUserSession
{
    public record RevokeUserSessionCommand : IRequest<Result>
    {
        public int UserSessionId { get; init; }

        public int UserId { get; init; }

        public RevokeUserSessionCommand(int userSessionId, int userId)
        {
            UserSessionId = userSessionId;
            UserId = userId;
        }
    }
}