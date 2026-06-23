using ESAM.GrowTracking.Application.Results;
using ESAM.GrowTracking.Domain.Enums;

namespace ESAM.GrowTracking.Application.Abstractions.Services
{
    public interface ISecurityValidatorService
    {
        Task<Result> ValidateAccessTokenTemporaryAsync(string currentJti, int currentUserId, string currentSecurityStamp, int currentTokenVersion, int currentUserDeviceId, 
            CancellationToken cancellationToken = default);

        Task<Result> ValidateAccessTokenSessionAsync(string currentJti, int currentUserId, string currentSecurityStamp, int currentTokenVersion, int currentUserDeviceId, 
            int currentUserSessionId, int currentWorkProfileId, WorkProfileType workProfileType, CancellationToken cancellationToken = default);

        Task<Result> ValidateAccessTokenSessionAsync(string currentJti, int currentUserId, string currentSecurityStamp, int currentTokenVersion, int currentUserDeviceId,
            int currentUserSessionId, int currentWorkProfileId, WorkProfileType workProfileType, int currentRoleId, int currentCampusId,
            CancellationToken cancellationToken = default);
    }
}