using ESAM.GrowTracking.Application.DTOs;
using ESAM.GrowTracking.Domain.Enums;

namespace ESAM.GrowTracking.Application.Abstractions.Services
{
    public interface ITokenService
    {
        AccessTokenDTO GenerateTemporaryAccessToken(int userId, string securityStamp, int tokenVersion, int userDeviceId, bool isPersistent, DateTime utcNow, int lifetimeMinutes);

        AccessTokenDTO GenerateSessionAccessToken(int userId, string securityStamp, int tokenVersion, int userDeviceId, int userSessionId, DateTime utcNow, int lifetimeMinutes,
            int workProfileSelectedId, int workProfileId, WorkProfileType workProfileType, int? roleCampusSelectedId = null, int? roleId = null, int? campusId = null);

        RefreshTokenDTO GenerateRefreshToken(DateTime utcNow, int lifetimeDays);

        Task<AccessTokenClaimsDTO> ExtractAccessTokenClaimsAsync(string accessToken);
    }

    //public interface ITokenService
    //{
    //    AccessTokenDTO GenerateTemporaryAccessToken(int userId, string securityStamp, int tokenVersion, int userDeviceId, bool isPersistent, DateTime utcNow, int lifetimeMinutes);

    //    AccessTokenDTO GenerateSessionAccessToken(int userId, string securityStamp, int tokenVersion, int userDeviceId, int userSessionId, DateTime utcNow, int lifetimeMinutes,
    //        int workProfileId, WorkProfileType workProfileType, int? roleId = null, int? campusId = null);

    //    RefreshTokenDTO GenerateRefreshToken(DateTime utcNow, int lifetimeDays);

    //    Task<AccessTokenClaimsDTO> ExtractAccessTokenClaimsAsync(string accessToken);
    //}
}