using ESAM.GrowTracking.Application.Results;

namespace ESAM.GrowTracking.Application.Abstractions.Services
{
    public interface ICurrentSessionIntegrityValidationService
    {
        Task<Result> ValidateCurrentUserStatusAsync(int currentUserId, DateTime utcNow, bool asTracking = false, CancellationToken cancellationToken = default);

        Task<Result> ValidateCurrentUserSecurityAsync(int currentUserId, string currentSecurityStamp, int currentTokenVersion, bool asTracking = false, 
            CancellationToken cancellationToken = default);

        Task<Result> ValidateCurrentUserAsync(int currentUserId, string currentSecurityStamp, int currentTokenVersion, DateTime utcNow, bool asTracking = false, 
            CancellationToken cancellationToken = default);

        Task<Result> ValidateCurrentUserDeviceStatusAsync(int currentUserDeviceId, int currentUserId, DateTime utcNow, bool asTracking = false, 
            CancellationToken cancellationToken = default);
    }
}