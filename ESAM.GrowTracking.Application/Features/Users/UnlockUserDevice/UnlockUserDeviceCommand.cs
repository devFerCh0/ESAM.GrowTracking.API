using ESAM.GrowTracking.Application.Results;
using MediatR;

namespace ESAM.GrowTracking.Application.Features.Users.UnlockUserDevice
{
    public record UnlockUserDeviceCommand : IRequest<Result>
    {
        public int UserId { get; init; }

        public int UserDeviceId { get; init; }

        public UnlockUserDeviceCommand(int userId, int userDeviceId)
        {
            UserId = userId;
            UserDeviceId = userDeviceId;
        }
    }
}