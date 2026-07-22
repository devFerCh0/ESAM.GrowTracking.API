using ESAM.GrowTracking.Application.Features.Commons;
using ESAM.GrowTracking.Application.Features.UserDevices.GetUserDevices;
using ESAM.GrowTracking.Domain.Entities;

namespace ESAM.GrowTracking.Application.Abstractions.DataAccess.Queries
{
    public interface IUserDeviceQuery : IQuery<UserDevice, int>
    {
        //Task<List<GetLockedUserDeviceResponse>> GetAllLockedByUserIdAsync(int userId, DateTime utcNow, bool asTracking = false, CancellationToken cancellationToken = default);

        Task<PagedResponse<GetUserDevicesResponse.UserDeviceResponse>> GetUserDevicesAsync(GetUserDevicesFilter userDevicesFilter, bool asTracking = false, 
            CancellationToken cancellationToken = default);
    }
}