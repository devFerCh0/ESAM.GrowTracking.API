using ESAM.GrowTracking.Application.Results;
using MediatR;

namespace ESAM.GrowTracking.Application.Features.Users.DeleteUser
{
    public record DeleteUserCommand : IRequest<Result>
    {
        public int UserId { get; init; }

        public DeleteUserCommand(int userId)
        {
            UserId = userId;
        }
    }
}