using ESAM.GrowTracking.Application.Enums;
using ESAM.GrowTracking.Application.Results;

namespace ESAM.GrowTracking.Application.Abstractions.Services
{
    public interface IAccessTokenValidationService
    {
        Task<Result> ValidateAccessTokenAsync(AccessTokenType accessTokenType, string currentJti, int currentUserId, string currentSecurityStamp, int currentTokenVersion, 
            int currentUserDeviceId, int? currentUserSessionId = null, int? currentWorkProfileId = null, int? currentRoleId = null, int? currentCampusId = null, 
            CancellationToken cancellationToken = default);
    }
}