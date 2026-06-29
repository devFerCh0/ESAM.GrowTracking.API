using ESAM.GrowTracking.Application.Abstractions.Services;
using ESAM.GrowTracking.Domain.Abstractions.DataAccess.Repositories;
using ESAM.GrowTracking.Domain.Entities;

namespace ESAM.GrowTracking.Application.Services
{
    public class BlacklistedTokenService : IBlacklistedTokenService
    {
        private readonly IBlacklistedAccessTokenTemporaryRepository _blacklistedAccessTokenTemporaryRepository;
        private readonly IBlacklistedRefreshTokenRepository _blacklistedRefreshTokenRepository;
        private readonly IBlacklistedAccessTokenPermanentRepository _blacklistedAccessTokenPermanentRepository;

        public BlacklistedTokenService(IBlacklistedAccessTokenTemporaryRepository blacklistedAccessTokenTemporaryRepository, 
            IBlacklistedRefreshTokenRepository blacklistedRefreshTokenRepository, IBlacklistedAccessTokenPermanentRepository blacklistedAccessTokenPermanentRepository)
        {
            ArgumentNullException.ThrowIfNull(blacklistedAccessTokenTemporaryRepository);
            ArgumentNullException.ThrowIfNull(blacklistedRefreshTokenRepository);
            ArgumentNullException.ThrowIfNull(blacklistedAccessTokenPermanentRepository);
            _blacklistedAccessTokenTemporaryRepository = blacklistedAccessTokenTemporaryRepository;
            _blacklistedRefreshTokenRepository = blacklistedRefreshTokenRepository;
            _blacklistedAccessTokenPermanentRepository = blacklistedAccessTokenPermanentRepository;
        }

        //public async Task<BlacklistedAccessTokenTemporary?> TryGenerateBlacklistedAccessTokenTemporaryAsync(int userId, string jti, DateTime expirationDate, DateTime blacklistedAt,
        //    string reason, int createdBy, DateTime utcNow, bool asTracking = false, CancellationToken cancellationToken = default)
        //{
        //    var isAlreadyBlacklisted = await _blacklistedAccessTokenTemporaryRepository.ExistsAsync(jti, asTracking, cancellationToken);
        //    return isAlreadyBlacklisted ? null : new BlacklistedAccessTokenTemporary(userId, jti, expirationDate, blacklistedAt, reason, createdBy, utcNow);
        //}

        //public async Task<List<BlacklistedRefreshToken>> GetPendingBlacklistedRefreshTokensAsync(List<(int Id, string Identifier, DateTime ExpiresAt)> userSessionRefreshTokens,
        //    DateTime blacklistedAt, string reason, int currentUserId, DateTime utcNow, bool asTracking = false, CancellationToken cancellationToken = default)
        //{
        //    var identifiers = userSessionRefreshTokens.Select(ust => ust.Identifier).ToList();
        //    var existingIdentifiers = await _blacklistedRefreshTokenRepository.GetExistingIdentifiersAsync(identifiers, asTracking, cancellationToken);
        //    var existingIdentifierSet = new HashSet<string>(existingIdentifiers, StringComparer.Ordinal);
        //    return [.. userSessionRefreshTokens.Where(ust => !existingIdentifierSet.Contains(ust.Identifier))
        //        .Select(usrt => new BlacklistedRefreshToken(usrt.Id, usrt.Identifier, usrt.ExpiresAt, blacklistedAt, reason, currentUserId, utcNow))];
        //}

        //public async Task<BlacklistedAccessTokenPermanent?> TryGenerateBlacklistedAccessTokenPermanentAsync(int userSessionId, string jti, DateTime expirationDate, 
        //    DateTime blacklistedAt, string reason, int createdBy, DateTime utcNow, bool asTracking = false, CancellationToken cancellationToken = default)
        //{
        //    var isAlreadyBlacklisted = await _blacklistedAccessTokenPermanentRepository.ExistsAsync(jti, asTracking, cancellationToken);
        //    return isAlreadyBlacklisted ? null : new BlacklistedAccessTokenPermanent(userSessionId, jti, expirationDate, blacklistedAt, reason, createdBy, utcNow);
        //}

        public async Task<BlacklistedRefreshToken?> TryGenerateBlacklistedRefreshTokenAsync(int userSessionRefreshTokenId, string identifier, DateTime expirationDate, 
            DateTime blacklistedAt, string reason, int createdBy, DateTime utcNow, bool asTracking = false, CancellationToken cancellationToken = default)
        {
            var isAlreadyBlacklisted = await _blacklistedRefreshTokenRepository.ExistsAsync(identifier, asTracking, cancellationToken);
            return isAlreadyBlacklisted ? null : new BlacklistedRefreshToken(userSessionRefreshTokenId, identifier, expirationDate, blacklistedAt, reason, createdBy, utcNow);
        }
    }
}