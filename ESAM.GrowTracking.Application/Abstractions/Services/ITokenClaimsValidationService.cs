using ESAM.GrowTracking.Application.Enums;

namespace ESAM.GrowTracking.Application.Abstractions.Services
{
    public interface ITokenClaimsValidationService
    {
        bool IsAuthenticated();

        int CurrentUserId();

        AccessTokenType CurrentAccessTokenType();

        string CurrentSecurityStamp();

        int CurrentTokenVersion();

        string CurrentJti();

        DateTime CurrentAccessTokenExpiration();

        int CurrentUserDeviceId();

        int CurrentUserSessionId();

        bool CurrentIsPersistent();

        int CurrentWorkProfileId();

        int CurrentRoleId();

        int CurrentCampusId();
    }
}