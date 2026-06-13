using ESAM.GrowTracking.Application.Results;

namespace ESAM.GrowTracking.Application.Abstractions.Services
{
    public interface ICurrentSessionValidationService
    {
        Task<Result> ValidateCurrentUserAsync(int currentUserId, string currentSecurityStamp, int currentTokenVersion, DateTime utcNow, bool asTracking = false, 
            CancellationToken cancellationToken = default);

        Task<Result> ValidateCurrentUserDeviceAsync(int currentUserDeviceId, int currentUserId, DateTime utcNow, bool asTracking = false, 
            CancellationToken cancellationToken = default);

        Task<Result> ValidateCurrentAccessTokenTemporaryAsync(string currentJti, bool asTracking = false, CancellationToken cancellationToken = default);

        //Task<Result> ValidateUserWorkProfileAsync(int currentUserId, int currentWorkProfileId, WorkProfileType workProfileType, bool asTracking = false, 
        //    CancellationToken cancellationToken = default);

        //Task<Result> ValidateUserWorkProfileAndPermissionsAsync(int currentUserId, int currentWorkProfileId, WorkProfileType workProfileType, bool asTracking = false, 
        //    CancellationToken cancellationToken = default);

        //Task<Result> ValidateCampusRoleContextAndPermissionsAsync(int currentUserId, int currentRoleId, int currentCampusId, bool asTracking = false, 
        //    CancellationToken cancellationToken = default);
    }
}