using ESAM.GrowTracking.Domain.Entities;

namespace ESAM.GrowTracking.Domain.Abstractions.DataAccess.Repositories
{
    public interface IRolePermissionRepository : IRepository<RolePermission>
    {
        Task<bool> HasActivePermissionsWithAccessAsync(int roleId, bool asTracking = false, CancellationToken cancellationToken = default);
    }
}