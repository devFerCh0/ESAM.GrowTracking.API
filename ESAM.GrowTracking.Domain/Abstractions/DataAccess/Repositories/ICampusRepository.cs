using ESAM.GrowTracking.Domain.Entities;

namespace ESAM.GrowTracking.Domain.Abstractions.DataAccess.Repositories
{
    public interface ICampusRepository : IRepository<Campus, int>
    {
        Task<bool> IsActiveAsync(int id, bool asTracking = false, CancellationToken cancellationToken = default);
    }
}