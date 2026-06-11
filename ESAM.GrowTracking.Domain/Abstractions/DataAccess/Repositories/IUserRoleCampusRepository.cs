using ESAM.GrowTracking.Domain.Entities;

namespace ESAM.GrowTracking.Domain.Abstractions.DataAccess.Repositories
{
    public interface IUserRoleCampusRepository : IRepository<UserRoleCampus>
    {
        //Task<UserRoleCampus?> GetByUserIdAndRoleIdAndCampusIdAsync(int userId, int roleId, int campusId, bool asTracking = false, CancellationToken cancellationToken = default);

        //Task<bool> IsActiveAsync(int userId, int roleId, int campusId, bool asTracking = false, CancellationToken cancellationToken = default);
    }
}