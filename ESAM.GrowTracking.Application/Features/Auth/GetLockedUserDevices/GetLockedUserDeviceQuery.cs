using ESAM.GrowTracking.Application.Results;
using MediatR;

namespace ESAM.GrowTracking.Application.Features.Auth.GetLockedUserDevices
{
    public record GetLockedUserDeviceQuery : IRequest<Result<List<GetLockedUserDeviceResponse>>>
    {
        public int UserId { get; init; }

        public GetLockedUserDeviceQuery(int userId)
        {
            UserId = userId;
        }
    }
}