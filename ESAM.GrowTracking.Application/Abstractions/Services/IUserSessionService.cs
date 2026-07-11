using ESAM.GrowTracking.Application.DTOs;
using ESAM.GrowTracking.Domain.Entities;

namespace ESAM.GrowTracking.Application.Abstractions.Services
{
    public interface IUserSessionService
    {
        Task<(RefreshTokenDTO RefreshToken, UserSession UserSession, int WorkProfileSelectedId, int RoleCampusSelectedId)> CreateUserSessionAsync(
            int currentUserId, int currentUserDeviceId, string? ipAddress, string? userAgent, bool isPersistent, int currentWorkProfileId, int currentRoleId, int currentCampusId, 
            string jti, DateTime accessTokenExpiration, DateTime utcNow, bool asTracking = false, CancellationToken cancellationToken = default);

        Task<(RefreshTokenDTO RefreshToken, UserSession UserSession, int WorkProfileSelectedId)> CreateUserSessionAsync(int currentUserId, int currentUserDeviceId, 
            string? ipAddress, string? userAgent, bool isPersistent, int currentWorkProfileId, string jti, DateTime accessTokenExpiration, DateTime utcNow, bool asTracking = false, 
            CancellationToken cancellationToken = default);

        Task RevokeUserSessionAndAccessTokenTemporaryAsync(UserSession userSession, string jti, DateTime accessTokenExpiration, string revokedReason, int currentUserId,
            DateTime utcNow, bool asTracking = false, CancellationToken cancellationToken = default);

        Task RevokeUserSessionAndAccessTokenSessionAsync(UserSession userSession, string jti, DateTime accessTokenExpiration, string revokedReason, int currentUserId,
            int currentUserSessionId, DateTime utcNow, bool asTracking = false, CancellationToken cancellationToken = default);

        Task RevokeUserSessionAndAccessTokenSessionAsync(UserSession? userSession1, UserSession? userSession2, string jti, DateTime accessTokenExpiration, string revokedReason,
            int currentUserId, int currentUserSessionId, DateTime utcNow, bool asTracking = false, CancellationToken cancellationToken = default);

        Task RevokeUserSessionAsync(UserSession userSession, string revokedReason, int currentUserId, DateTime utcNow, bool asTracking = false,
            CancellationToken cancellationToken = default);

        Task<int> RevokeUserSessionsAsync(IReadOnlyCollection<UserSession> userSessions, string revokedReason, int currentUserId, DateTime utcNow, bool asTracking = false,
            CancellationToken cancellationToken = default);

        Task RevokeAccessTokenTemporaryAsync(int currentUserId, string currentJti, DateTime currentAccessTokenExpiration, string reason, DateTime utcNow, bool asTracking = false,
            CancellationToken cancellationToken = default);

        Task RevokeAccessTokenSessionAsync(int currentUserSessionId, string currentJti, DateTime currentAccessTokenExpiration, int currentUserId, string reason, DateTime utcNow,
            bool asTracking = false, CancellationToken cancellationToken = default);

        Task<RefreshTokenDTO> RotateUserSessionAsync(UserSession userSession, UserSessionRefreshToken userSessionRefreshToken, string jti, DateTime accessTokenExpiration,
            string revokedReason, int currentUserId, DateTime utcNow, bool asTracking = false, CancellationToken cancellationToken = default);

        Task<int> ChangeRoleCampusAsync(UserSession userSession, int currentWorkProfileSelectedId, int currentRoleCampusSelectedId, int roleId, int campusId, string currentJti, 
            DateTime currentAccessTokenExpiration, string revokedReason, int currentUserId, DateTime utcNow, bool asTracking = false, 
            CancellationToken cancellationToken = default);
    }

    //public interface IUserSessionService
    //{
    //    Task<(RefreshTokenDTO, UserSession)> CreateUserSessionAsync(int currentUserId, int currentUserDeviceId, string? ipAddress, string? userAgent, bool isPersistent, 
    //        int currentWorkProfileId, int currentRoleId, int currentCampusId, string jti, DateTime accessTokenExpiration, DateTime utcNow, bool asTracking = false,
    //        CancellationToken cancellationToken = default);

    //    Task<(RefreshTokenDTO, UserSession)> CreateUserSessionAsync(int currentUserId, int currentUserDeviceId, string? ipAddress, string? userAgent, bool isPersistent,
    //        int currentWorkProfileId, string jti, DateTime accessTokenExpiration, DateTime utcNow, bool asTracking = false, CancellationToken cancellationToken = default);

    //    Task RevokeUserSessionAndAccessTokenTemporaryAsync(UserSession userSession, string jti, DateTime accessTokenExpiration, string revokedReason, int currentUserId, 
    //        DateTime utcNow, bool asTracking = false, CancellationToken cancellationToken = default);

    //    Task RevokeUserSessionAndAccessTokenSessionAsync(UserSession userSession, string jti, DateTime accessTokenExpiration, string revokedReason, int currentUserId, 
    //        int currentUserSessionId, DateTime utcNow, bool asTracking = false, CancellationToken cancellationToken = default);

    //    Task RevokeUserSessionAndAccessTokenSessionAsync(UserSession? userSession1, UserSession? userSession2, string jti, DateTime accessTokenExpiration, string revokedReason, 
    //        int currentUserId, int currentUserSessionId, DateTime utcNow, bool asTracking = false, CancellationToken cancellationToken = default);

    //    Task RevokeUserSessionAsync(UserSession userSession, string revokedReason, int currentUserId, DateTime utcNow, bool asTracking = false, 
    //        CancellationToken cancellationToken = default);

    //    Task<int> RevokeUserSessionsAsync(IReadOnlyCollection<UserSession> userSessions, string revokedReason, int currentUserId, DateTime utcNow, bool asTracking = false,
    //        CancellationToken cancellationToken = default);

    //    Task RevokeAccessTokenTemporaryAsync(int currentUserId, string currentJti, DateTime currentAccessTokenExpiration, string reason, DateTime utcNow, bool asTracking = false, 
    //        CancellationToken cancellationToken = default);

    //    Task RevokeAccessTokenSessionAsync(int currentUserSessionId, string currentJti, DateTime currentAccessTokenExpiration, int currentUserId, string reason, DateTime utcNow, 
    //        bool asTracking = false, CancellationToken cancellationToken = default);

    //    Task<RefreshTokenDTO> RotateUserSessionAsync(UserSession userSession, UserSessionRefreshToken userSessionRefreshToken, string jti, DateTime accessTokenExpiration, 
    //        string revokedReason, int currentUserId, DateTime utcNow, bool asTracking = false, CancellationToken cancellationToken = default);
    //}
}