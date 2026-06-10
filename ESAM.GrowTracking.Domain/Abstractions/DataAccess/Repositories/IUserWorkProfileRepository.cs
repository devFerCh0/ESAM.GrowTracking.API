using ESAM.GrowTracking.Domain.Entities;

namespace ESAM.GrowTracking.Domain.Abstractions.DataAccess.Repositories
{
    public interface IUserWorkProfileRepository : IRepository<UserWorkProfile>
    {
        Task<UserWorkProfile?> GetByUserIdAndWorkProfileIdAsync(int userId, int workProfileId, bool asTracking = false, CancellationToken cancellationToken = default);

        Task<bool> IsActiveAsync(int userId, int workProfileId, bool asTracking = false, CancellationToken cancellationToken = default);
    }
}