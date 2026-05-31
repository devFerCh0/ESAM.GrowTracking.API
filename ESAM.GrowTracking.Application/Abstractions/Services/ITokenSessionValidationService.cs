using ESAM.GrowTracking.Application.Abstractions.Services.Results;
using ESAM.GrowTracking.Domain.Entities;

namespace ESAM.GrowTracking.Application.Abstractions.Services
{
    public interface ITokenSessionValidationService
    {
        Task<TokenSessionValidationResult> ValidateAsync(UserSession userSession, UserSessionRefreshToken userSessionRefreshToken, string? refreshTokenPlain,
            string? deviceIdentifier, string revokedReasonPrefix, bool isAuthenticated, int currentUserId, int? currentWorkProfileId, int? currentRoleId, int? currentCampusId,
            int? currentTokenVersion, string? currentSecurityStamp, DateTime utcNow, bool asTracking = false, CancellationToken cancellationToken = default);
    }
}