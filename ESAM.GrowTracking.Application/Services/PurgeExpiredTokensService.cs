using ESAM.GrowTracking.Application.Abstractions.Services;
using ESAM.GrowTracking.Application.Abstractions.Services.Results;
using ESAM.GrowTracking.Domain.Abstractions.DataAccess.Repositories;
using Microsoft.Extensions.Logging;

namespace ESAM.GrowTracking.Application.Services
{
    public sealed class PurgeExpiredTokensService : IPurgeExpiredTokensService
    {
        private readonly ILogger<PurgeExpiredTokensService> _logger;
        private readonly IBlacklistedAccessTokenTemporaryRepository _blacklistedAccessTokenTemporaryRepository;
        private readonly IBlacklistedAccessTokenPermanentRepository _blacklistedAccessTokenPermanentRepository;
        private readonly IBlacklistedRefreshTokenRepository _blacklistedRefreshTokenRepository;
        private readonly IUserSessionRefreshTokenRepository _userSessionRefreshTokenRepository;

        public PurgeExpiredTokensService(ILogger<PurgeExpiredTokensService> logger, IBlacklistedAccessTokenTemporaryRepository blacklistedAccessTokenTemporaryRepository,
            IBlacklistedAccessTokenPermanentRepository blacklistedAccessTokenPermanentRepository, IBlacklistedRefreshTokenRepository blacklistedRefreshTokenRepository,
            IUserSessionRefreshTokenRepository userSessionRefreshTokenRepository)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(blacklistedAccessTokenTemporaryRepository);
            ArgumentNullException.ThrowIfNull(blacklistedAccessTokenPermanentRepository);
            ArgumentNullException.ThrowIfNull(blacklistedRefreshTokenRepository);
            ArgumentNullException.ThrowIfNull(userSessionRefreshTokenRepository);
            _logger = logger;
            _blacklistedAccessTokenTemporaryRepository = blacklistedAccessTokenTemporaryRepository;
            _blacklistedAccessTokenPermanentRepository = blacklistedAccessTokenPermanentRepository;
            _blacklistedRefreshTokenRepository = blacklistedRefreshTokenRepository;
            _userSessionRefreshTokenRepository = userSessionRefreshTokenRepository;
        }

        public async Task<PurgeExpiredTokensResult> PurgeAsync(int batchSize, DateTime utcNow, CancellationToken cancellationToken = default)
        {
            var temporaryDeleted = await _blacklistedAccessTokenTemporaryRepository.PurgeExpiredBlacklistedAccessTokensTemporaryAsync(batchSize, utcNow, cancellationToken);
            _logger.LogInformation("PurgeExpiredTokensService: eliminados {Count} BlacklistedAccessTokensTemporary expirados. UtcNow={UtcNow}", temporaryDeleted, utcNow);
            var permanentDeleted = await _blacklistedAccessTokenPermanentRepository.PurgeExpiredBlacklistedAccessTokensPermanentAsync(batchSize, utcNow, cancellationToken);
            _logger.LogInformation("PurgeExpiredTokensService: eliminados {Count} BlacklistedAccessTokensPermanent expirados. UtcNow={UtcNow}", permanentDeleted, utcNow);
            var blacklistedRefreshDeleted = await _blacklistedRefreshTokenRepository.PurgeExpiredBlacklistedRefreshTokensAsync(batchSize, utcNow, cancellationToken);
            _logger.LogInformation("PurgeExpiredTokensService: eliminados {Count} BlacklistedRefreshTokens expirados. UtcNow={UtcNow}", blacklistedRefreshDeleted, utcNow);
            var sessionRefreshDeleted = await _userSessionRefreshTokenRepository.PurgeExpiredUserSessionRefreshTokensAsync(batchSize, utcNow, cancellationToken);
            _logger.LogInformation("PurgeExpiredTokensService: eliminados {Count} UserSessionRefreshTokens expirados. UtcNow={UtcNow}", sessionRefreshDeleted, utcNow);
            return new PurgeExpiredTokensResult(temporaryDeleted, permanentDeleted, blacklistedRefreshDeleted, sessionRefreshDeleted, utcNow);
        }
    }
}