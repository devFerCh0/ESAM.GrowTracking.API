using ESAM.GrowTracking.Application.Abstractions.Services;
using ESAM.GrowTracking.Domain.Abstractions.DataAccess.Repositories;
using ESAM.GrowTracking.Domain.Entities;

namespace ESAM.GrowTracking.Application.Services
{
    public class UserSessionRevocationService : IUserSessionRevocationService
    {
        private readonly IUserSessionRepository _userSessionRepository;
        private readonly IUserSessionRefreshTokenRepository _userSessionRefreshTokenRepository;
        private readonly IBlacklistedRefreshTokenRepository _blacklistedRefreshTokenRepository;

        public UserSessionRevocationService(IUserSessionRepository userSessionRepository, IUserSessionRefreshTokenRepository userSessionRefreshTokenRepository, 
            IBlacklistedRefreshTokenRepository blacklistedRefreshTokenRepository)
        {
            ArgumentNullException.ThrowIfNull(userSessionRepository);
            ArgumentNullException.ThrowIfNull(userSessionRefreshTokenRepository);
            ArgumentNullException.ThrowIfNull(blacklistedRefreshTokenRepository);
            _userSessionRepository = userSessionRepository;
            _userSessionRefreshTokenRepository = userSessionRefreshTokenRepository;
            _blacklistedRefreshTokenRepository = blacklistedRefreshTokenRepository;
        }

        public async Task<(List<UserSession> UserSessionsToRevoke, List<UserSessionRefreshToken> UserSessionRefreshTokensToRevoke, 
                List<BlacklistedRefreshToken> BlacklistedRefreshTokens)>
            RevokeUserSessionsAsync(int userId, string revokedReason, int currentUserId, DateTime utcNow, bool asTracking = false, CancellationToken cancellationToken = default)
        {
            var userSessions = await _userSessionRepository.GetActiveByUserIdAsync(userId, utcNow, asTracking, cancellationToken);
            var sessionsToRevoke = new List<UserSession>();
            var refreshTokensToRevoke = new List<UserSessionRefreshToken>();
            var blacklistedRefreshTokens = new List<BlacklistedRefreshToken>();
            if (userSessions is not null)
                foreach (var userSession in userSessions)
                {
                    var (sessionToRevoke, refreshTokens, blacklisted) = await PrepareSessionRevocationAsync(userSession, revokedReason, currentUserId, utcNow, asTracking,
                        cancellationToken);
                    if (sessionToRevoke is not null)
                        sessionsToRevoke.Add(sessionToRevoke);
                    refreshTokensToRevoke.AddRange(refreshTokens);
                    blacklistedRefreshTokens.AddRange(blacklisted);
                }
            return (sessionsToRevoke, refreshTokensToRevoke, blacklistedRefreshTokens);
        }

        private async
            Task<(UserSession? UserSessionToRevoke, List<UserSessionRefreshToken> UserSessionRefreshTokensToRevoke, List<BlacklistedRefreshToken> BlacklistedRefreshTokens)>
            PrepareSessionRevocationAsync(UserSession userSession, string revokedReason, int currentUserId, DateTime utcNow, bool asTracking, CancellationToken cancellationToken)
        {
            UserSession? userSessionToRevoke = null;
            if (!userSession.IsRevoked)
            {
                userSession.Revoke(utcNow, revokedReason, currentUserId, currentUserId, utcNow);
                userSession.UpdateLastActivity(utcNow, currentUserId, utcNow);
                userSessionToRevoke = userSession;
            }
            var userSessionRefreshTokens = await _userSessionRefreshTokenRepository.GetAllByUserSessionIdAsync(userSession.Id, asTracking, cancellationToken);
            var userSessionRefreshTokensToRevoke = userSessionRefreshTokens.Where(usrt => !usrt.IsRevoked).ToList();
            foreach (var userSessionRefreshTokenToRevoke in userSessionRefreshTokensToRevoke)
            {
                userSessionRefreshTokenToRevoke.Revoke(utcNow, revokedReason, currentUserId, utcNow);
                userSessionRefreshTokenToRevoke.UpdateLastUsedAt(utcNow, currentUserId, utcNow);
            }
            var identifiers = userSessionRefreshTokens.Select(ust => ust.Identifier).ToList();
            var existingIdentifiers = await _blacklistedRefreshTokenRepository.GetExistingIdentifiersAsync(identifiers, asTracking, cancellationToken);
            var existingIdentifierSet = new HashSet<string>(existingIdentifiers, StringComparer.Ordinal);
            var blacklistedRefreshTokens = userSessionRefreshTokens.Where(ust => !existingIdentifierSet.Contains(ust.Identifier))
                .Select(usrt => new BlacklistedRefreshToken(usrt.Id, usrt.Identifier, usrt.ExpiresAt, utcNow, revokedReason, currentUserId, utcNow)).ToList();
            return (userSessionToRevoke, userSessionRefreshTokensToRevoke, blacklistedRefreshTokens);
        }
    }
}
