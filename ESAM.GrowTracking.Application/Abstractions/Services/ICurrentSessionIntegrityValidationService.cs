using ESAM.GrowTracking.Application.Results;
using ESAM.GrowTracking.Domain.Enums;

namespace ESAM.GrowTracking.Application.Abstractions.Services
{
    public interface ICurrentSessionIntegrityValidationService
    {
        Task<Result> ValidateUserContextAsync(int currentUserId, string currentSecurityStamp, int currentTokenVersion, DateTime utcNow, bool asTracking = false, 
            CancellationToken cancellationToken = default);

        Task<Result> ValidateUserDeviceAsync(int currentUserDeviceId, int currentUserId, DateTime utcNow, bool asTracking = false, CancellationToken cancellationToken = default);

        Task<Result> ValidateWorkProfileContextAsync(int currentUserId, int currentWorkProfileId, WorkProfileType workProfileType, bool asTracking = false, 
            CancellationToken cancellationToken = default);

        Task<Result> ValidateWorkProfileContextAndPermissionsAsync(int currentUserId, int currentWorkProfileId, WorkProfileType workProfileType, bool asTracking = false, 
            CancellationToken cancellationToken = default);

        Task<Result> ValidateCampusRoleContextAndPermissionsAsync(int currentUserId, int currentRoleId, int currentCampusId, bool asTracking = false, 
            CancellationToken cancellationToken = default);
    }
}