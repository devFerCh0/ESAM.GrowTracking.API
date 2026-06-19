using ESAM.GrowTracking.Application.Features.Auth.GetUserRoleCampuses;
using ESAM.GrowTracking.Domain.Entities;

namespace ESAM.GrowTracking.Application.Abstractions.DataAccess.Queries
{
    public interface IUserRoleCampusQuery : IQuery<UserRoleCampus>
    {
        Task<List<GetUserRoleCampusResponse>> GetUserRoleCampusesByUserIdAsync(int userId, bool asTracking = false, CancellationToken cancellationToken = default);
    }
}