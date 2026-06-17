using ESAM.GrowTracking.Application.Results;
using ESAM.GrowTracking.Domain.Enums;

namespace ESAM.GrowTracking.Application.Abstractions.Services
{
    public interface IAccessTokenValidationService
    {
        Task<Result> ValidateCurrentTemporaryAsync(string currentJti, int currentUserId, string currentSecurityStamp, int currentTokenVersion, int currentUserDeviceId,
            CancellationToken cancellationToken = default);

        Task<Result> ValidateCurrentSessionAsync(string currentJti, int currentUserId, string currentSecurityStamp, int currentTokenVersion, int currentUserDeviceId,
            int currentUserSessionId, int currentWorkProfileId, WorkProfileType workProfileType, int currentRoleId, int currentCampusId, 
            CancellationToken cancellationToken = default);
    }
}