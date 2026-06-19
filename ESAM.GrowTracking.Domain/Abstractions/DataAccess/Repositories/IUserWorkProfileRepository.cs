using ESAM.GrowTracking.Domain.Entities;
using ESAM.GrowTracking.Domain.Enums;

namespace ESAM.GrowTracking.Domain.Abstractions.DataAccess.Repositories
{
    public interface IUserWorkProfileRepository : IRepository<UserWorkProfile>
    {
        Task<bool> IsActiveAndOfTypeAsync(int userId, int workProfileId, WorkProfileType workProfileType, bool asTracking = false, CancellationToken cancellationToken = default);

        //Task<UserWorkProfile?> GetByUserIdAndWorkProfileIdAsync(int userId, int workProfileId, bool asTracking = false, CancellationToken cancellationToken = default);
    }
}