using ESAM.GrowTracking.Application.Abstractions.Services.Results;

namespace ESAM.GrowTracking.Application.Abstractions.Services
{
    public interface IAuthSessionIntegrityValidatorService
    {
        Task<AuthSessionIntegrityResult> ValidateTemporaryAccessTokenAsync(string operationContext, string? refreshTokenRaw, string currentJti,
            DateTime currentAccessTokenExpiration, int currentUserId, DateTime utcNow, bool asTracking, CancellationToken cancellationToken);

        Task<AuthSessionIntegrityResult> ValidateSessionAccessTokenAsync(string operationContext, string? refreshTokenRaw, string currentJti, DateTime currentAccessTokenExpiration, 
            int currentUserId, int currentUserSessionId, int currentUserDeviceId, DateTime utcNow, bool asTracking, CancellationToken cancellationToken);
    }
}