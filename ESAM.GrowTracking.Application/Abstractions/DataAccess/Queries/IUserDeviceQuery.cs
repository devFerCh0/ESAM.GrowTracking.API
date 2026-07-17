using ESAM.GrowTracking.Application.Features.Auth.GetLockedUserDevices;
using ESAM.GrowTracking.Domain.Entities;

namespace ESAM.GrowTracking.Application.Abstractions.DataAccess.Queries
{
    public interface IUserDeviceQuery : IQuery<UserDevice, int>
    {
        Task<List<GetLockedUserDeviceResponse>> GetAllLockedByUserIdAsync(int userId, DateTime utcNow, bool asTracking = false, CancellationToken cancellationToken = default);
    }
}