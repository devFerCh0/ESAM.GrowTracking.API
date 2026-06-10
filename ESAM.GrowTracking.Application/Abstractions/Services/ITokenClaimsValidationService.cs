using ESAM.GrowTracking.Application.Enums;

namespace ESAM.GrowTracking.Application.Abstractions.Services
{
    public interface ITokenClaimsValidationService
    {
        bool GetIsAuthenticated { get; }

        AccessTokenType GetCurrentAccessTokenType { get; }

        int GetCurrentUserId { get; }

        string GetCurrentSecurityStamp { get; }

        int GetCurrentTokenVersion { get; }

        string GetCurrentJti { get; }

        DateTime GetCurrentAccessTokenExpiration { get; }

        int GetCurrentUserDeviceId { get; }

        int GetCurrentUserSessionId { get; }

        bool GetCurrentIsPersistent { get; }

        int GetCurrentWorkProfileId { get; }

        int GetCurrentRoleId { get; }

        int GetCurrentCampusId { get; }
    }
}