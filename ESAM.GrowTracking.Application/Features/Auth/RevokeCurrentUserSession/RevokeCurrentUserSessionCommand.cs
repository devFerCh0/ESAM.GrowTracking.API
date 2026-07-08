using ESAM.GrowTracking.Application.Results;
using MediatR;

namespace ESAM.GrowTracking.Application.Features.Auth.RevokeCurrentUserSession
{
    public record RevokeCurrentUserSessionCommand : IRequest<Result>
    {
        public int UserSessionId { get; init; }

        public RevokeCurrentUserSessionCommand(int userSessionId)
        {
            UserSessionId = userSessionId;
        }
    }
}