using ESAM.GrowTracking.Domain.Entities;

namespace ESAM.GrowTracking.Domain.Abstractions.DataAccess.Repositories
{
    public interface IUserSessionWorkProfileSelectedRepository : IRepository<UserSessionWorkProfileSelected>
    {
        //Task<UserSessionWorkProfileSelected?> GetByUserSessionIdAsync(int userSessionId, bool asTracking = false, CancellationToken cancellationToken = default);
    }
}