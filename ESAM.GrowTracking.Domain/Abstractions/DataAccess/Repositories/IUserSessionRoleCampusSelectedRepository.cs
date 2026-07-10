using ESAM.GrowTracking.Domain.Entities;

namespace ESAM.GrowTracking.Domain.Abstractions.DataAccess.Repositories
{
    public interface IUserSessionRoleCampusSelectedRepository : IRepository<UserSessionRoleCampusSelected, int> { }

    //public interface IUserSessionRoleCampusSelectedRepository : IRepository<UserSessionRoleCampusSelected>
    //{
    //    //Task<UserSessionRoleCampusSelected?> GetByUserSessionIdAsync(int userSessionId, bool asTracking = false, CancellationToken cancellationToken = default);
    //}
}