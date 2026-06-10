using ESAM.GrowTracking.Application.Results;
using ESAM.GrowTracking.Domain.Entities;
using ESAM.GrowTracking.Domain.Enums;

namespace ESAM.GrowTracking.Application.Abstractions.Services
{
    public interface ICurrentUserValidatorService
    {
        //Task<Result> ValidateCurrentUserAsync(int currentUserId, DateTime utcNow, bool asTracking = false, CancellationToken cancellationToken = default);

        //Task<Result<User>> GetAndValidateCurrentUserAsync(int currentUserId, DateTime utcNow, bool asTracking = false, CancellationToken cancellationToken = default);

        Task<Result> ValidateCurrentUserDeviceAsync(int currentUserId, int currentUserDeviceId, DateTime utcNow, bool asTracking = false, 
            CancellationToken cancellationToken = default);

        Task<Result<UserDevice>> GetAndValidateCurrentUserDeviceAsync(int currentUserId, int currentUserDeviceId, DateTime utcNow, bool asTracking = false,
            CancellationToken cancellationToken = default);

        Task<Result> ValidateUserWorkProfileAndTypeAsync(int currentUserId, int currentWorkProfileId, WorkProfileType workProfileType, bool asTracking = false, 
            CancellationToken cancellationToken = default);

        Task<Result> ValidateUserWorkProfileAndTypeAndHasPermissionsAsync(int currentUserId, int currentWorkProfileId, WorkProfileType workProfileType, bool asTracking = false, 
            CancellationToken cancellationToken = default);

        Task<Result> ValidateUserRoleCampusAndHasPermissionsAsync(int currentUserId, int currentRoleId, int currentCampusId, bool asTracking = false, 
            CancellationToken cancellationToken = default);
    }
}