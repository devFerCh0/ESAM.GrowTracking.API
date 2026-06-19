using ESAM.GrowTracking.Application.Results;
using ESAM.GrowTracking.Domain.Enums;

namespace ESAM.GrowTracking.Application.Abstractions.Services
{
    public interface ISecurityValidatorService
    {
        Task<Result> ValidateBlacklistedAccessTokenTemporaryAsync(string jti, bool asTracking, CancellationToken cancellationToken);

        Task<Result> ValidateUserAsync(int userId, string securityStamp, int tokenVersion, DateTime utcNow, bool asTracking, CancellationToken cancellationToken);

        Task<Result> ValidateUserDeviceAsync(int userId, int userDeviceId, DateTime utcNow, bool asTracking, CancellationToken cancellationToken);

        Task<Result> ValidateBlacklistedAccessTokenSessionAsync(string jti, bool asTracking, CancellationToken cancellationToken);

        Task<Result> ValidateUserWorkProfileAsync(int userId, int workProfileId, WorkProfileType workProfileType, bool asTracking, CancellationToken cancellationToken);

        Task<Result> ValidateWorkProfilePermissionsAsync(int workProfileId, bool asTracking, CancellationToken cancellationToken);

        Task<Result> ValidateUseRoleCampusAsync(int userId, int roleId, int campusId, bool asTracking, CancellationToken cancellationToken);

        Task<Result> ValidateRolePermissionAsync(int currentRoleId, bool asTracking, CancellationToken cancellationToken);

        Task<Result> ValidateAccessTokenTemporaryAsync(string currentJti, int currentUserId, string currentSecurityStamp, int currentTokenVersion, int currentUserDeviceId, 
            CancellationToken cancellationToken = default);

        Task<Result> ValidateAccessTokenSessionAsync(string currentJti, int currentUserId, string currentSecurityStamp, int currentTokenVersion, int currentUserDeviceId, 
            int currentUserSessionId, int currentWorkProfileId, WorkProfileType workProfileType, int currentRoleId, int currentCampusId, 
            CancellationToken cancellationToken = default);
    }
}