using ESAM.GrowTracking.Domain.Entities;

namespace ESAM.GrowTracking.Domain.Abstractions.DataAccess.Repositories
{
    public interface IRoleRepository : IRepository<Role, int>
    {
        Task<bool> IsActiveAsync(int id, bool asTracking = false, CancellationToken cancellationToken = default);
    }
}