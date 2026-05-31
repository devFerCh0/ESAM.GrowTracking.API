using ESAM.GrowTracking.Domain.Entities;

namespace ESAM.GrowTracking.Application.Abstractions.Services
{
    public interface IBlacklistedTokenService
    {
        Task<BlacklistedAccessTokenTemporary?> TryGenerateBlacklistedAccessTokenTemporaryAsync(int userId, string jti, DateTime expirationDate, DateTime blacklistedAt, 
            string reason, int createdBy, DateTime utcNow, bool asTracking = false, CancellationToken cancellationToken = default);

        Task<List<BlacklistedRefreshToken>> GetPendingBlacklistedRefreshTokensAsync(List<(int Id, string Identifier, DateTime ExpiresAt)> userSessionRefreshTokens, 
            DateTime blacklistedAt, string reason, int currentUserId, DateTime utcNow, bool asTracking = false, CancellationToken cancellationToken = default);

        Task<BlacklistedAccessTokenPermanent?> TryGenerateBlacklistedAccessTokenPermanentAsync(int userSessionId, string jti, DateTime expirationDate, DateTime blacklistedAt,
            string reason, int createdBy, DateTime utcNow, bool asTracking = false, CancellationToken cancellationToken = default);

        Task<BlacklistedRefreshToken?> TryGenerateBlacklistedRefreshTokenAsync(int userSessionRefreshTokenId, string tokenIdentifier, DateTime expirationDate,
            DateTime blacklistedAt, string reason, int createdBy, DateTime utcNow, bool asTracking = false, CancellationToken cancellationToken = default);
    }
}