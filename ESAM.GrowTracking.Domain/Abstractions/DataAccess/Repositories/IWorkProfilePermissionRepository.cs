using ESAM.GrowTracking.Domain.Entities;

namespace ESAM.GrowTracking.Domain.Abstractions.DataAccess.Repositories
{
    public interface IWorkProfilePermissionRepository : IRepository<WorkProfilePermission>
    {
        //Task<bool> HasActivePermissionsWithAccessAsync(int workProfileId, bool asTracking = false, CancellationToken cancellationToken = default);
    }
}