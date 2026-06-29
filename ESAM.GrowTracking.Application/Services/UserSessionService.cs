using ESAM.GrowTracking.Application.Abstractions.Services;
using ESAM.GrowTracking.Application.DTOs;
using ESAM.GrowTracking.Application.Results;
using ESAM.GrowTracking.Application.Settings;
using ESAM.GrowTracking.Application.ValueObjects;
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
            var (refreshToken, tokenSalt, tokenHash) = GenerateRefreshTokenWithHash(utcNow);
            var userSession = new UserSession(currentUserId, currentUserDeviceId, ipAddress, userAgent, utcNow.AddDays(_tokenLifetimeSettings.SessionIdleWindowDays),
                utcNow.AddDays(_tokenLifetimeSettings.SessionAbsoluteLifetimeDays), isPersistent, currentUserId, utcNow);
            var userSessionWorkProfileSelected = new UserSessionWorkProfileSelected(currentUserId, currentWorkProfileId);
            var userSessionRoleCampusSelected = new UserSessionRoleCampusSelected(currentUserId, currentRoleId, currentCampusId);
            userSession.UpdateLastActivity(utcNow, currentUserId, utcNow);
            var userSessionRefreshToken = new UserSessionRefreshToken(refreshToken.Identifier, tokenSalt, tokenHash, refreshToken.ExpiresAt, currentUserId, utcNow);
            userSessionRefreshToken.UpdateLastUsedAt(utcNow, currentUserId, utcNow);
            var blacklistedAccessTokenTemporary = new BlacklistedAccessTokenTemporary(currentUserId, jti, accessTokenExpiration, utcNow,
                "Inicio de sesión asumido (Autenticado): Access token temporal revocado.", currentUserId, utcNow);
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
            var (refreshToken, tokenSalt, tokenHash) = GenerateRefreshTokenWithHash(utcNow);
            var userSession = new UserSession(currentUserId, currentUserDeviceId, ipAddress, userAgent, utcNow.AddDays(_tokenLifetimeSettings.SessionIdleWindowDays),
                utcNow.AddDays(_tokenLifetimeSettings.SessionAbsoluteLifetimeDays), isPersistent, currentUserId, utcNow);
            var userSessionWorkProfileSelected = new UserSessionWorkProfileSelected(currentUserId, currentWorkProfileId);
            userSession.UpdateLastActivity(utcNow, currentUserId, utcNow);
            var userSessionRefreshToken = new UserSessionRefreshToken(refreshToken.Identifier, tokenSalt, tokenHash, refreshToken.ExpiresAt, currentUserId, utcNow);
            userSessionRefreshToken.UpdateLastUsedAt(utcNow, currentUserId, utcNow);
            var blacklistedAccessTokenTemporary = new BlacklistedAccessTokenTemporary(currentUserId, jti, accessTokenExpiration, utcNow, 
                "Inicio de sesión asumido (Autenticado): Access token temporal revocado.", currentUserId, utcNow);
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

        public async Task RevokeUserSessionAsync(UserSession userSession, string jti, DateTime accessTokenExpiration, string revokedReason, int currentUserId,
            DateTime utcNow, bool asTracking = false, CancellationToken cancellationToken = default)
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
            List<BlacklistedRefreshToken> blacklistedRefreshTokens = [.. userSessionRefreshTokens.Where(ust => !existingIdentifierSet.Contains(ust.Identifier))
                .Select(usrt => new BlacklistedRefreshToken(usrt.Id, usrt.Identifier, usrt.ExpiresAt, utcNow, revokedReason, currentUserId, utcNow))];
            var blacklistedAccessTokenSession = new BlacklistedAccessTokenSession(userSession.Id, jti, accessTokenExpiration, utcNow, revokedReason, currentUserId, utcNow);
            await _unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                if (userSessionToRevoke is not null)
                    await _unitOfWork.UserSessions.UpdateAsync(userSessionToRevoke, ct);
                if (userSessionRefreshTokensToRevoke.Count > 0)
                    await _unitOfWork.UserSessionRefreshTokens.UpdateRangeAsync(userSessionRefreshTokensToRevoke, ct);
                if (blacklistedRefreshTokens.Count > 0)
                    await _unitOfWork.BlacklistedRefreshTokens.InsertRangeAsync(blacklistedRefreshTokens, ct);
                if (blacklistedAccessTokenSession is not null)
                    await _unitOfWork.BlacklistedAccessTokensSession.InsertAsync(blacklistedAccessTokenSession, ct);
            }, cancellationToken: cancellationToken);
        }

        //public async Task<RefreshTokenDTO> RotateUserSessionAsync(UserSession userSession, UserSessionRefreshToken userSessionRefreshToken, string? jti, 
        //    DateTime? accessTokenExpiration, string revokedReason, int currentUserId, DateTime utcNow, bool asTracking = false, CancellationToken cancellationToken = default)
        //{
        //    var (refreshToken, tokenSalt, tokenHash) = GenerateRefreshTokenWithHash(utcNow);
        //    var newUserSessionRefreshToken = new UserSessionRefreshToken(refreshToken.Identifier, tokenSalt, tokenHash, refreshToken.ExpiresAt, userSession.UserId, userSession.Id, 
        //        userSessionRefreshToken.RotationCount + 1, utcNow);
        //    newUserSessionRefreshToken.UpdateLastUsedAt(utcNow, userSession.UserId, utcNow);
        //    userSession.UpdateExpiresAt(utcNow.AddDays(_tokenLifetimeSettings.SessionIdleWindowDays), currentUserId, utcNow);
        //    userSession.UpdateLastActivity(utcNow, currentUserId, utcNow);
        //    userSessionRefreshToken.Revoke(utcNow, revokedReason, currentUserId, utcNow);
        //    userSessionRefreshToken.UpdateLastUsedAt(utcNow, currentUserId, utcNow);
        //    var blacklistedRefreshToken = await _blacklistedTokenService.TryGenerateBlacklistedRefreshTokenAsync(userSessionRefreshToken.Id, userSessionRefreshToken.Identifier, 
        //        userSessionRefreshToken.ExpiresAt, utcNow, revokedReason, currentUserId, utcNow, asTracking, cancellationToken);
        //    var blacklistedAccessTokenPermanent = (jti is not null && accessTokenExpiration is not null) ? 
        //        await _blacklistedTokenService.TryGenerateBlacklistedAccessTokenPermanentAsync(userSession.Id, jti, accessTokenExpiration.Value, utcNow, revokedReason, 
        //        currentUserId, utcNow, asTracking, cancellationToken) : null;
        //    await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        //    {
        //        await _unitOfWork.UserSessionRefreshTokens.InsertAsync(newUserSessionRefreshToken, ct);
        //        await _unitOfWork.SaveChangesAsync(ct);
        //        await _unitOfWork.UserSessions.UpdateAsync(userSession, ct);
        //        userSessionRefreshToken.UpdateReplacedByUserSessionRefreshTokenId(newUserSessionRefreshToken.Id);
        //        await _unitOfWork.UserSessionRefreshTokens.UpdateAsync(userSessionRefreshToken, ct);
        //        if (blacklistedRefreshToken is not null)
        //            await _unitOfWork.BlacklistedRefreshTokens.InsertAsync(blacklistedRefreshToken, ct);
        //        if (blacklistedAccessTokenPermanent is not null)
        //            await _unitOfWork.BlacklistedAccessTokensPermanent.InsertAsync(blacklistedAccessTokenPermanent, ct);
        //    }, cancellationToken: cancellationToken);
        //    return refreshToken;
        //}

        private (RefreshTokenDTO, string, string) GenerateRefreshTokenWithHash(DateTime utcNow)
        {
            var refreshToken = _tokenService.GenerateRefreshToken(utcNow, _tokenLifetimeSettings.RefreshTokenLifetimeDays);
            var tokenSalt = _hashService.GenerateSalt();
            var tokenHash = _hashService.ComputeHash(refreshToken.Token, tokenSalt);
            return (refreshToken, tokenSalt, tokenHash);
        }
    }
}