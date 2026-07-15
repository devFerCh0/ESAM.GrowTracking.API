using ESAM.GrowTracking.Domain.Entities;
namespace ESAM.GrowTracking.Domain.Abstractions.DataAccess.Repositories
{
    public interface IWorkProfileRepository : IRepository<WorkProfile, int>
    {
        //Task<bool> IsValidWorkProfileTypeAsync(int id, WorkProfileType workProfileType, bool asTracking = false, CancellationToken cancellationToken = default);

        //Task<WorkProfileType> GetWorkProfileTypeByIdAsync(int id, bool asTracking = false, CancellationToken cancellationToken = default);
        
        //Task<bool> IsActiveAndOfTypeAsync(int id, WorkProfileType workProfileType, bool asTracking = false, CancellationToken cancellationToken = default);
    }
}