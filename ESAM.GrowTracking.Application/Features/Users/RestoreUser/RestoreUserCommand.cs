using ESAM.GrowTracking.Application.Results;
using MediatR;

namespace ESAM.GrowTracking.Application.Features.Users.RestoreUser
{
    public record RestoreUserCommand : IRequest<Result>
    {
        public int UserId { get; init; }

        public RestoreUserCommand(int userId)
        {
            UserId = userId;
        }
    }
}