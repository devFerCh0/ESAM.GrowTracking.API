using ESAM.GrowTracking.Application.Abstractions.Services;
using ESAM.GrowTracking.Domain.Abstractions.DataAccess;
using ESAM.GrowTracking.Domain.Abstractions.DataAccess.Repositories;
using ESAM.GrowTracking.Domain.Entities;
using MediatR;

namespace ESAM.GrowTracking.Application.Services
{
    public class UserService : IUserService
    {
        //private readonly IUserSessionRepository _userSessionRepository;
        //private readonly IUserSessionRefreshTokenRepository _userSessionRefreshTokenRepository;
        //private readonly IBlacklistedRefreshTokenRepository _blacklistedRefreshTokenRepository;
        //private readonly IUnitOfWork _unitOfWork;

        //public UserService(IUserSessionRepository userSessionRepository, IUserSessionRefreshTokenRepository userSessionRefreshTokenRepository, 
        //    IBlacklistedRefreshTokenRepository blacklistedRefreshTokenRepository, IUnitOfWork unitOfWork)
        //{
        //    ArgumentNullException.ThrowIfNull(userSessionRepository);
        //    ArgumentNullException.ThrowIfNull(userSessionRefreshTokenRepository);
        //    ArgumentNullException.ThrowIfNull(blacklistedRefreshTokenRepository);
        //    ArgumentNullException.ThrowIfNull(unitOfWork);
        //    _userSessionRepository = userSessionRepository;
        //    _userSessionRefreshTokenRepository = userSessionRefreshTokenRepository;
        //    _blacklistedRefreshTokenRepository = blacklistedRefreshTokenRepository;
        //    _unitOfWork = unitOfWork;
        //}

        public void UserLock(User user, DateTime lockoutEndAt, int currentUserId, DateTime utcNow)
        {
            user.Lock(lockoutEndAt, currentUserId, utcNow);
            user.UpdateSecurityCredentials(Guid.NewGuid().ToString(), user.TokenVersion + 1, currentUserId, utcNow);
        }

        public void UserUnlock(User user, int currentUserId, DateTime utcNow)
        {
            user.Unlock(currentUserId, utcNow);
        }

        //public async Task LockUserAsync(User user, string revokedReason, int currentUserId, DateTime utcNow, bool asTracking = false, CancellationToken cancellationToken = default)
        //{
        //    var userSessions = await _userSessionRepository.GetActiveByUserIdAsync(user.Id, utcNow, asTracking, cancellationToken);
        //    var sessionsToRevoke = new List<UserSession>();
        //    var refreshTokensToRevoke = new List<UserSessionRefreshToken>();
        //    var blacklistedRefreshTokens = new List<BlacklistedRefreshToken>();
        //    if (userSessions is not null)
        //        foreach (var userSession in userSessions)
        //        {
        //            var (sessionToRevoke, refreshTokens, blacklisted) = await PrepareSessionRevocationAsync(userSession, revokedReason, currentUserId, utcNow, asTracking,
        //                cancellationToken);
        //            if (sessionToRevoke is not null)
        //                sessionsToRevoke.Add(sessionToRevoke);
        //            refreshTokensToRevoke.AddRange(refreshTokens);
        //            blacklistedRefreshTokens.AddRange(blacklisted);
        //        }
        //    await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        //    {
        //        if (sessionsToRevoke.Count > 0)
        //            await _unitOfWork.UserSessions.UpdateRangeAsync(sessionsToRevoke, ct);
        //        if (refreshTokensToRevoke.Count > 0)
        //            await _unitOfWork.UserSessionRefreshTokens.UpdateRangeAsync(refreshTokensToRevoke, ct);
        //        if (blacklistedRefreshTokens.Count > 0)
        //            await _unitOfWork.BlacklistedRefreshTokens.InsertRangeAsync(blacklistedRefreshTokens, ct);
        //        await _unitOfWork.Users.UpdateAsync(user, ct);
        //    }, cancellationToken: cancellationToken);
        //}

        //private async
        //    Task<(UserSession? UserSessionToRevoke, List<UserSessionRefreshToken> UserSessionRefreshTokensToRevoke, List<BlacklistedRefreshToken> BlacklistedRefreshTokens)>
        //    PrepareSessionRevocationAsync(UserSession userSession, string revokedReason, int currentUserId, DateTime utcNow, bool asTracking, CancellationToken cancellationToken)
        //{
        //    UserSession? userSessionToRevoke = null;
        //    if (!userSession.IsRevoked)
        //    {
        //        userSession.Revoke(utcNow, revokedReason, currentUserId, currentUserId, utcNow);
        //        userSession.UpdateLastActivity(utcNow, currentUserId, utcNow);
        //        userSessionToRevoke = userSession;
        //    }
        //    var userSessionRefreshTokens = await _userSessionRefreshTokenRepository.GetAllByUserSessionIdAsync(userSession.Id, asTracking, cancellationToken);
        //    var userSessionRefreshTokensToRevoke = userSessionRefreshTokens.Where(usrt => !usrt.IsRevoked).ToList();
        //    foreach (var userSessionRefreshTokenToRevoke in userSessionRefreshTokensToRevoke)
        //    {
        //        userSessionRefreshTokenToRevoke.Revoke(utcNow, revokedReason, currentUserId, utcNow);
        //        userSessionRefreshTokenToRevoke.UpdateLastUsedAt(utcNow, currentUserId, utcNow);
        //    }
        //    var identifiers = userSessionRefreshTokens.Select(ust => ust.Identifier).ToList();
        //    var existingIdentifiers = await _blacklistedRefreshTokenRepository.GetExistingIdentifiersAsync(identifiers, asTracking, cancellationToken);
        //    var existingIdentifierSet = new HashSet<string>(existingIdentifiers, StringComparer.Ordinal);
        //    var blacklistedRefreshTokens = userSessionRefreshTokens.Where(ust => !existingIdentifierSet.Contains(ust.Identifier))
        //        .Select(usrt => new BlacklistedRefreshToken(usrt.Id, usrt.Identifier, usrt.ExpiresAt, utcNow, revokedReason, currentUserId, utcNow)).ToList();
        //    return (userSessionToRevoke, userSessionRefreshTokensToRevoke, blacklistedRefreshTokens);
        //}
    }
}