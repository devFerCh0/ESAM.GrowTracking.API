using ESAM.GrowTracking.Application.Results;
using MediatR;

namespace ESAM.GrowTracking.Application.Features.Users.LockUser
{
    public record LockUserCommand : IRequest<Result>
    {
        public int UserId { get; init; }

        public DateTime LockoutEndAt { get; init; }

        public LockUserCommand(int userId, DateTime lockoutEndAt)
        {
            UserId = userId;
            LockoutEndAt = lockoutEndAt;
        }
    }
}