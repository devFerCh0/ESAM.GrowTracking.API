using ESAM.GrowTracking.Application.Abstractions.Services;
using ESAM.GrowTracking.Application.DTOs;
using ESAM.GrowTracking.Application.Settings;
using ESAM.GrowTracking.Domain.Abstractions.DataAccess;
using ESAM.GrowTracking.Domain.Abstractions.DataAccess.Repositories;
using ESAM.GrowTracking.Domain.Entities;
using Microsoft.Extensions.Options;

namespace ESAM.GrowTracking.Application.Services
{
    public class UserSessionService : IUserSessionService
    {
        private readonly ITokenService _tokenService;
        private readonly TokenLifetimeSettings _tokenLifetimeSettings;
        private readonly IHashService _hashService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserSessionRefreshTokenRepository _userSessionRefreshTokenRepository;
        private readonly IBlacklistedRefreshTokenRepository _blacklistedRefreshTokenRepository;

        public UserSessionService(ITokenService tokenService, IOptions<TokenLifetimeSettings> tokenLifetimeSettingsOptions, IHashService hashService, IUnitOfWork unitOfWork, 
            IUserSessionRefreshTokenRepository userSessionRefreshTokenRepository, IBlacklistedRefreshTokenRepository blacklistedRefreshTokenRepository)
        {
            ArgumentNullException.ThrowIfNull(tokenService);
            ArgumentNullException.ThrowIfNull(tokenLifetimeSettingsOptions);
            ArgumentNullException.ThrowIfNull(hashService);
            ArgumentNullException.ThrowIfNull(unitOfWork);
            ArgumentNullException.ThrowIfNull(userSessionRefreshTokenRepository);
            ArgumentNullException.ThrowIfNull(blacklistedRefreshTokenRepository);
            _tokenService = tokenService;
            _tokenLifetimeSettings = tokenLifetimeSettingsOptions.Value ?? throw new ArgumentNullException(nameof(tokenLifetimeSettingsOptions));
            _hashService = hashService;
            _unitOfWork = unitOfWork;
            _userSessionRefreshTokenRepository = userSessionRefreshTokenRepository;
            _blacklistedRefreshTokenRepository = blacklistedRefreshTokenRepository;
        }

        public async Task<(RefreshTokenDTO, UserSession)> CreateUserSessionAsync(int currentUserId, int currentUserDeviceId, string? ipAddress, string? userAgent, 
            bool isPersistent, int currentWorkProfileId, int currentRoleId, int currentCampusId, string jti, DateTime accessTokenExpiration, DateTime utcNow, 
            CancellationToken cancellationToken = default)
        {
            var (userSession, userSessionWorkProfileSelected, userSessionRefreshToken, blacklistedAccessTokenTemporary, refreshToken) = PrepareSessionCreationAsync(currentUserId, 
                currentUserDeviceId, ipAddress, userAgent, isPersistent, currentWorkProfileId, jti, accessTokenExpiration, utcNow);
            var userSessionRoleCampusSelected = new UserSessionRoleCampusSelected(currentUserId, currentRoleId, currentCampusId);
            await _unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                await _unitOfWork.UserSessions.InsertAsync(userSession, ct);
                await _unitOfWork.SaveChangesAsync(ct);
                userSessionWorkProfileSelected.AddUserSessionId(userSession.Id);
                await _unitOfWork.UserSessionWorkProfilesSelected.InsertAsync(userSessionWorkProfileSelected, ct);
                userSessionRoleCampusSelected.AddUserSessionId(userSession.Id);
                await _unitOfWork.UserSessionRoleCampusesSelected.InsertAsync(userSessionRoleCampusSelected, ct);
                userSessionRefreshToken.AddUserSessionId(userSession.Id);
                await _unitOfWork.UserSessionRefreshTokens.InsertAsync(userSessionRefreshToken, ct);
                await _unitOfWork.BlacklistedAccessTokensTemporary.InsertAsync(blacklistedAccessTokenTemporary, ct);
            }, cancellationToken: cancellationToken);
            return (refreshToken, userSession);
        }

        public async Task<(RefreshTokenDTO, UserSession)> CreateUserSessionAsync(int currentUserId, int currentUserDeviceId, string? ipAddress, string? userAgent, 
            bool isPersistent, int currentWorkProfileId, string jti, DateTime accessTokenExpiration, DateTime utcNow, CancellationToken cancellationToken = default)
        {
            var (userSession, userSessionWorkProfileSelected, userSessionRefreshToken, blacklistedAccessTokenTemporary, refreshToken) = PrepareSessionCreationAsync(currentUserId,
                currentUserDeviceId, ipAddress, userAgent, isPersistent, currentWorkProfileId, jti, accessTokenExpiration, utcNow);
            await _unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                await _unitOfWork.UserSessions.InsertAsync(userSession, ct);
                await _unitOfWork.SaveChangesAsync(ct);
                userSessionWorkProfileSelected.AddUserSessionId(userSession.Id);
                await _unitOfWork.UserSessionWorkProfilesSelected.InsertAsync(userSessionWorkProfileSelected, ct);
                userSessionRefreshToken.AddUserSessionId(userSession.Id);
                await _unitOfWork.UserSessionRefreshTokens.InsertAsync(userSessionRefreshToken, ct);
                await _unitOfWork.BlacklistedAccessTokensTemporary.InsertAsync(blacklistedAccessTokenTemporary, ct);
            }, cancellationToken: cancellationToken);
            return (refreshToken, userSession);
        }

        public async Task RevokeUserSessionAsync(UserSession userSession, string revokedReason, int currentUserId, DateTime utcNow, bool asTracking = false, 
            CancellationToken cancellationToken = default)
        {
            var (userSessionToRevoke, userSessionRefreshTokensToRevoke, blacklistedRefreshTokens) = await PrepareSessionRevocationAsync(userSession, revokedReason, currentUserId, 
                utcNow, asTracking, cancellationToken);
            await _unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                if (userSessionToRevoke is not null)
                    await _unitOfWork.UserSessions.UpdateAsync(userSessionToRevoke, ct);
                if (userSessionRefreshTokensToRevoke.Count > 0)
                    await _unitOfWork.UserSessionRefreshTokens.UpdateRangeAsync(userSessionRefreshTokensToRevoke, ct);
                if (blacklistedRefreshTokens.Count > 0)
                    await _unitOfWork.BlacklistedRefreshTokens.InsertRangeAsync(blacklistedRefreshTokens, ct);
            }, cancellationToken: cancellationToken);
        }

        public async Task RevokeUserSessionAndAccessTokenTemporaryAsync(UserSession userSession, string jti, DateTime accessTokenExpiration, string revokedReason, 
            int currentUserId, DateTime utcNow, bool asTracking = false, CancellationToken cancellationToken = default)
        {
            var (userSessionToRevoke, userSessionRefreshTokensToRevoke, blacklistedRefreshTokens) = await PrepareSessionRevocationAsync(userSession, revokedReason, currentUserId, 
                utcNow, asTracking, cancellationToken);
            var blacklistedAccessTokenTemporary = new BlacklistedAccessTokenTemporary(currentUserId, jti, accessTokenExpiration, utcNow, revokedReason, currentUserId, utcNow);
            await _unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                if (userSessionToRevoke is not null)
                    await _unitOfWork.UserSessions.UpdateAsync(userSessionToRevoke, ct);
                if (userSessionRefreshTokensToRevoke.Count > 0)
                    await _unitOfWork.UserSessionRefreshTokens.UpdateRangeAsync(userSessionRefreshTokensToRevoke, ct);
                if (blacklistedRefreshTokens.Count > 0)
                    await _unitOfWork.BlacklistedRefreshTokens.InsertRangeAsync(blacklistedRefreshTokens, ct);
                await _unitOfWork.BlacklistedAccessTokensTemporary.InsertAsync(blacklistedAccessTokenTemporary, ct);
            }, cancellationToken: cancellationToken);
        }

        public async Task RevokeUserSessionAndAccessTokenSessionAsync(UserSession userSession, string jti, DateTime accessTokenExpiration, string revokedReason, int currentUserId, 
            int currentUserSessionId, DateTime utcNow, bool asTracking = false, CancellationToken cancellationToken = default)
        {
            var (userSessionToRevoke, userSessionRefreshTokensToRevoke, blacklistedRefreshTokens) = await PrepareSessionRevocationAsync(userSession, revokedReason, currentUserId, 
                utcNow, asTracking, cancellationToken);
            var blacklistedAccessTokenSession = new BlacklistedAccessTokenSession(currentUserSessionId, jti, accessTokenExpiration, utcNow, revokedReason, currentUserId, utcNow);
            await _unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                if (userSessionToRevoke is not null)
                    await _unitOfWork.UserSessions.UpdateAsync(userSessionToRevoke, ct);
                if (userSessionRefreshTokensToRevoke.Count > 0)
                    await _unitOfWork.UserSessionRefreshTokens.UpdateRangeAsync(userSessionRefreshTokensToRevoke, ct);
                if (blacklistedRefreshTokens.Count > 0)
                    await _unitOfWork.BlacklistedRefreshTokens.InsertRangeAsync(blacklistedRefreshTokens, ct);
                await _unitOfWork.BlacklistedAccessTokensSession.InsertAsync(blacklistedAccessTokenSession, ct);
            }, cancellationToken: cancellationToken);
        }

        public async Task RevokeUserSessionAndAccessTokenSessionAsync(UserSession? userSession1, UserSession? userSession2, string jti, DateTime accessTokenExpiration, 
            string revokedReason, int currentUserId, int currentUserSessionId, DateTime utcNow, bool asTracking = false, CancellationToken cancellationToken = default)
        {
            var (userSessionToRevoke1, userSessionRefreshTokensToRevoke1, blacklistedRefreshTokens1) = userSession1 is not null 
                ? await PrepareSessionRevocationAsync(userSession1, revokedReason, currentUserId, utcNow, asTracking, cancellationToken) : (null, [], []);
            var (userSessionToRevoke2, userSessionRefreshTokensToRevoke2, blacklistedRefreshTokens2) = userSession2 is not null
                ? await PrepareSessionRevocationAsync(userSession2, revokedReason, currentUserId, utcNow, asTracking, cancellationToken) : (null, [], []);
            var blacklistedAccessTokenSession = new BlacklistedAccessTokenSession(currentUserSessionId, jti, accessTokenExpiration, utcNow, revokedReason, currentUserId, utcNow);
            await _unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                if (userSessionToRevoke1 is not null)
                    await _unitOfWork.UserSessions.UpdateAsync(userSessionToRevoke1, ct);
                if (userSessionToRevoke2 is not null)
                    await _unitOfWork.UserSessions.UpdateAsync(userSessionToRevoke2, ct);
                if (userSessionRefreshTokensToRevoke1.Count > 0) 
                    await _unitOfWork.UserSessionRefreshTokens.UpdateRangeAsync(userSessionRefreshTokensToRevoke1, ct);
                if (userSessionRefreshTokensToRevoke2.Count > 0)
                    await _unitOfWork.UserSessionRefreshTokens.UpdateRangeAsync(userSessionRefreshTokensToRevoke2, ct);
                if (blacklistedRefreshTokens1.Count > 0) 
                    await _unitOfWork.BlacklistedRefreshTokens.InsertRangeAsync(blacklistedRefreshTokens1, ct);
                if (blacklistedRefreshTokens2.Count > 0)
                    await _unitOfWork.BlacklistedRefreshTokens.InsertRangeAsync(blacklistedRefreshTokens2, ct);
                await _unitOfWork.BlacklistedAccessTokensSession.InsertAsync(blacklistedAccessTokenSession, ct);
            }, cancellationToken: cancellationToken);
        }

        public async Task BlacklistedAccessTokenTemporaryAsync(int currentUserId, string currentJti, DateTime currentAccessTokenExpiration, string reason, DateTime utcNow, 
            CancellationToken cancellationToken = default)
        {
            var blacklistedAccessTokenTemporary = new BlacklistedAccessTokenTemporary(currentUserId, currentJti, currentAccessTokenExpiration, utcNow, 
                reason, currentUserId, utcNow);
            await _unitOfWork.BlacklistedAccessTokensTemporary.InsertAsync(blacklistedAccessTokenTemporary, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task BlacklistedAccessTokenSessionAsync(int currentUserSessionId, string currentJti, DateTime currentAccessTokenExpiration, int currentUserId, string reason, 
            DateTime utcNow, CancellationToken cancellationToken = default)
        {
            var blacklistedAccessTokenSession = new BlacklistedAccessTokenSession(currentUserSessionId, currentJti, currentAccessTokenExpiration, utcNow, reason, currentUserId, 
                utcNow);
            await _unitOfWork.BlacklistedAccessTokensSession.InsertAsync(blacklistedAccessTokenSession, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task<RefreshTokenDTO> RotateUserSessionAsync(UserSession userSession, UserSessionRefreshToken userSessionRefreshToken, string jti, 
            DateTime accessTokenExpiration, string revokedReason, int currentUserId, DateTime utcNow, bool asTracking = false, CancellationToken cancellationToken = default)
        {
            var (refreshToken, tokenSalt, tokenHash) = GenerateRefreshTokenWithHash(utcNow);
            var newUserSessionRefreshToken = new UserSessionRefreshToken(userSession.Id, refreshToken.Identifier, tokenSalt, tokenHash, refreshToken.ExpiresAt, userSession.UserId, 
                utcNow);
            newUserSessionRefreshToken.UpdateLastUsedAt(utcNow, userSession.UserId, utcNow);
            userSession.UpdateExpiresAt(utcNow.AddDays(_tokenLifetimeSettings.SessionIdleWindowDays), currentUserId, utcNow);
            userSession.UpdateLastActivity(utcNow, currentUserId, utcNow);
            userSessionRefreshToken.Revoke(utcNow, revokedReason, currentUserId, utcNow);
            userSessionRefreshToken.UpdateLastUsedAt(utcNow, currentUserId, utcNow);
            var blacklistedRefreshToken = new BlacklistedRefreshToken(userSessionRefreshToken.Id, userSessionRefreshToken.Identifier, userSessionRefreshToken.ExpiresAt,
                utcNow, revokedReason, currentUserId, utcNow);
            var blacklistedAccessTokenSession = new BlacklistedAccessTokenSession(userSession.Id, jti, accessTokenExpiration, utcNow, revokedReason, currentUserId, utcNow);

            await _unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                await _unitOfWork.UserSessionRefreshTokens.InsertAsync(newUserSessionRefreshToken, ct);
                await _unitOfWork.SaveChangesAsync(ct);
                await _unitOfWork.UserSessions.UpdateAsync(userSession, ct);
                userSessionRefreshToken.UpdateReplacedByUserSessionRefreshTokenId(newUserSessionRefreshToken.Id);
                await _unitOfWork.UserSessionRefreshTokens.UpdateAsync(userSessionRefreshToken, ct);
                await _unitOfWork.BlacklistedRefreshTokens.InsertAsync(blacklistedRefreshToken, ct);
                await _unitOfWork.BlacklistedAccessTokensSession.InsertAsync(blacklistedAccessTokenSession, ct);
            }, cancellationToken: cancellationToken);
            return refreshToken;
        }

        private (UserSession UserSession, UserSessionWorkProfileSelected UserSessionWorkProfileSelected, UserSessionRefreshToken UserSessionRefreshToken, 
            BlacklistedAccessTokenTemporary BlacklistedAccessTokenTemporary, RefreshTokenDTO RefreshToken) PrepareSessionCreationAsync(int currentUserId, int currentUserDeviceId, 
                string? ipAddress, string? userAgent, bool isPersistent, int currentWorkProfileId, string jti, DateTime accessTokenExpiration, DateTime utcNow)
        {
            var (refreshToken, tokenSalt, tokenHash) = GenerateRefreshTokenWithHash(utcNow);
            var userSession = new UserSession(currentUserId, currentUserDeviceId, ipAddress, userAgent, utcNow.AddDays(_tokenLifetimeSettings.SessionIdleWindowDays),
                utcNow.AddDays(_tokenLifetimeSettings.SessionAbsoluteLifetimeDays), isPersistent, currentUserId, utcNow);
            var userSessionWorkProfileSelected = new UserSessionWorkProfileSelected(currentUserId, currentWorkProfileId);
            userSession.UpdateLastActivity(utcNow, currentUserId, utcNow);
            var userSessionRefreshToken = new UserSessionRefreshToken(refreshToken.Identifier, tokenSalt, tokenHash, refreshToken.ExpiresAt, currentUserId, utcNow);
            userSessionRefreshToken.UpdateLastUsedAt(utcNow, currentUserId, utcNow);
            var blacklistedAccessTokenTemporary = new BlacklistedAccessTokenTemporary(currentUserId, jti, accessTokenExpiration, utcNow,
                "Inicio de sesión asumido (Autenticado): Access token temporal revocado.", currentUserId, utcNow);
            return (userSession, userSessionWorkProfileSelected, userSessionRefreshToken, blacklistedAccessTokenTemporary, refreshToken);
        }

        private (RefreshTokenDTO, string, string) GenerateRefreshTokenWithHash(DateTime utcNow)
        {
            var refreshToken = _tokenService.GenerateRefreshToken(utcNow, _tokenLifetimeSettings.RefreshTokenLifetimeDays);
            var tokenSalt = _hashService.GenerateSalt();
            var tokenHash = _hashService.ComputeHash(refreshToken.Token, tokenSalt);
            return (refreshToken, tokenSalt, tokenHash);
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