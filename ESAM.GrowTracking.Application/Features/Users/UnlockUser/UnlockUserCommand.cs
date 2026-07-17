using ESAM.GrowTracking.Application.Results;
using MediatR;

namespace ESAM.GrowTracking.Application.Features.Users.UnlockUser
{
    public record UnlockUserCommand : IRequest<Result>
    {
        public int UserId { get; init; }

        public UnlockUserCommand(int userId)
        {
            UserId = userId;
        }
    }
}