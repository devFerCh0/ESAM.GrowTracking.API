using ESAM.GrowTracking.Application.DTOs;
using ESAM.GrowTracking.Domain.Entities;
using ESAM.GrowTracking.Domain.Enums;

namespace ESAM.GrowTracking.Application.Abstractions.Services
{
    public interface IUserSessionService
    {
        Task<(RefreshTokenDTO, UserSession)> CreateUserSessionAsync(int currentUserId, int currentUserDeviceId, string? ipAddress, string? userAgent, int currentWorkProfileId,
            DateTime utcNow, WorkProfileType workProfileType, string jti, DateTime accessTokenExpiration, bool isPersistent, int currentRoleId = 0, int currentCampusId = 0,
            bool asTracking = false, CancellationToken cancellationToken = default);

        Task RevokeUserSessionAsync(UserSession userSession, string? jti, DateTime? accessTokenExpiration, string revokedReason, int currentUserId, DateTime utcNow, 
            bool asTracking = false, CancellationToken cancellationToken = default);

        Task<RefreshTokenDTO> RotateUserSessionAsync(UserSession userSession, UserSessionRefreshToken userSessionRefreshToken, string? jti, DateTime? accessTokenExpiration,
            string revokedReason, int currentUserId, DateTime utcNow, bool asTracking = false, CancellationToken cancellationToken = default);
    }
}