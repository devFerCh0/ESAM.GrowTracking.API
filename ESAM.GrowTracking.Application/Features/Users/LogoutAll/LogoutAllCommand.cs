using ESAM.GrowTracking.Application.Results;
using MediatR;

namespace ESAM.GrowTracking.Application.Features.Users.LogoutAll
{
    public record LogoutAllCommand : IRequest<Result<LogoutAllResponse>>
    {
        public int UserId { get; init; }

        public LogoutAllCommand(int userId)
        {
            UserId = userId;
        }
    }
}