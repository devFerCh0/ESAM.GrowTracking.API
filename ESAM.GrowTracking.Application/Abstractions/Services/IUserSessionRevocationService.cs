using ESAM.GrowTracking.Domain.Entities;

namespace ESAM.GrowTracking.Application.Abstractions.Services
{
    public interface IUserSessionRevocationService
    {
        Task<(List<UserSession> UserSessionsToRevoke, List<UserSessionRefreshToken> UserSessionRefreshTokensToRevoke, List<BlacklistedRefreshToken> BlacklistedRefreshTokens)> 
            RevokeUserSessionsAsync(int userId, string revokedReason, int currentUserId, DateTime utcNow, bool asTracking = false, CancellationToken cancellationToken = default);
    }
}