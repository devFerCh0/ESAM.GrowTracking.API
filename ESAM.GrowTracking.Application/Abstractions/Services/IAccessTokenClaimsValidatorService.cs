using ESAM.GrowTracking.Application.Enums;
using ESAM.GrowTracking.Domain.Enums;

namespace ESAM.GrowTracking.Application.Abstractions.Services
{
    public interface IAccessTokenClaimsValidatorService
    {
        bool IsAuthenticated { get; }

        AccessTokenType CurrentAccessTokenType { get; }

        int CurrentUserId { get; }

        string CurrentSecurityStamp { get; }

        int CurrentTokenVersion { get; }

        string CurrentJti { get; }

        DateTime CurrentAccessTokenExpiration { get; }

        int CurrentUserDeviceId { get; }

        int CurrentUserSessionId { get; }

        bool CurrentIsPersistent { get; }

        int CurrentWorkProfileSelectedId { get; }

        int CurrentWorkProfileId { get; }

        WorkProfileType CurrentWorkProfileType { get; }

        int CurrentRoleCampusSelectedId { get; }

        int CurrentRoleId { get; }

        int CurrentCampusId { get; }

        Task UseExplicitAccessTokenAsync(string accessToken);
    }

    //public interface IAccessTokenClaimsValidatorService
    //{
    //    bool IsAuthenticated { get; }

    //    AccessTokenType CurrentAccessTokenType { get; }

    //    int CurrentUserId { get; }

    //    string CurrentSecurityStamp { get; }

    //    int CurrentTokenVersion { get; }

    //    string CurrentJti { get; }

    //    DateTime CurrentAccessTokenExpiration { get; }

    //    int CurrentUserDeviceId { get; }

    //    int CurrentUserSessionId { get; }

    //    bool CurrentIsPersistent { get; }

    //    int CurrentWorkProfileId { get; }

    //    WorkProfileType CurrentWorkProfileType { get; }

    //    int CurrentRoleId { get; }

    //    int CurrentCampusId { get; }

    //    Task UseExplicitAccessTokenAsync(string accessToken);
    //}
}