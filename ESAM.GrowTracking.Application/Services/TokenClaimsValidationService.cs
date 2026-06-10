using ESAM.GrowTracking.Application.Abstractions.Services;
using ESAM.GrowTracking.Application.Enums;
using Microsoft.Extensions.Logging;

namespace ESAM.GrowTracking.Application.Services
{
    public class TokenClaimsValidationService : ITokenClaimsValidationService
    {
        private readonly ILogger<TokenClaimsValidationService> _logger;
        private readonly ICurrentUserService _currentUserService;

        public TokenClaimsValidationService(ILogger<TokenClaimsValidationService> logger, ICurrentUserService currentUserService)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(currentUserService);
            _logger = logger;
            _currentUserService = currentUserService;
        }

        public bool GetIsAuthenticated => _currentUserService.IsAuthenticated;

        public AccessTokenType GetCurrentAccessTokenType
        {
            get
            {
                var currentAccessTokenType = _currentUserService.AccessTokenType;
                if (currentAccessTokenType is null)
                    throw new ArgumentNullException(nameof(currentAccessTokenType));
                return currentAccessTokenType.Value;
            }
        }

        public int GetCurrentUserId
        {
            get
            {
                var currentUserId = _currentUserService.UserId;
                if (currentUserId is null)
                    throw new ArgumentNullException(nameof(currentUserId));
                if (currentUserId <= 0)
                    throw new ArgumentOutOfRangeException(nameof(currentUserId));
                return currentUserId.Value;
            }
        }

        public string GetCurrentSecurityStamp
        {
            get
            {
                var currentSecurityStamp = _currentUserService.SecurityStamp;
                if (string.IsNullOrWhiteSpace(currentSecurityStamp))
                    throw new ArgumentException(nameof(currentSecurityStamp));
                return currentSecurityStamp;
            }
        }

        public int GetCurrentTokenVersion
        {
            get
            {
                var currentTokenVersion = _currentUserService.TokenVersion;
                if (currentTokenVersion is null)
                    throw new ArgumentNullException(nameof(currentTokenVersion));
                if (currentTokenVersion < 0)
                    throw new ArgumentOutOfRangeException(nameof(currentTokenVersion));
                return currentTokenVersion.Value;
            }
        }

        public string GetCurrentJti
        {
            get
            {
                var currentJti = _currentUserService.Jti;
                if (string.IsNullOrWhiteSpace(currentJti))
                    throw new ArgumentException(nameof(currentJti));
                return currentJti;
            }
        }

        public DateTime GetCurrentAccessTokenExpiration
        {
            get
            {
                var currentAccessTokenExpiration = _currentUserService.AccessTokenExpiration;
                if (currentAccessTokenExpiration is null)
                    throw new ArgumentException(nameof(currentAccessTokenExpiration));
                return currentAccessTokenExpiration.Value;
            }
        }

        public int GetCurrentUserDeviceId
        {
            get
            {
                var currentUserDeviceId = _currentUserService.UserDeviceId;
                if (currentUserDeviceId is null)
                    throw new ArgumentNullException(nameof(currentUserDeviceId));
                if (currentUserDeviceId <= 0)
                    throw new ArgumentOutOfRangeException(nameof(currentUserDeviceId));
                return currentUserDeviceId.Value;
            }
        }

        public int GetCurrentUserSessionId
        {
            get
            {
                var currentUserSessionId = _currentUserService.UserSessionId;
                if (currentUserSessionId is null)
                    throw new ArgumentNullException(nameof(currentUserSessionId));
                if (currentUserSessionId <= 0)
                    throw new ArgumentOutOfRangeException(nameof(currentUserSessionId));
                return currentUserSessionId.Value;
            }
        }

        public bool GetCurrentIsPersistent
        {
            get
            {
                var currentIsPersistent = _currentUserService.IsPersistent;
                if (currentIsPersistent is null)
                    throw new ArgumentException(nameof(currentIsPersistent));
                return currentIsPersistent.Value;
            }
        }

        public int GetCurrentWorkProfileId
        {
            get
            {
                var currentWorkProfileId = _currentUserService.WorkProfileId;
                if (currentWorkProfileId is null)
                    throw new ArgumentNullException(nameof(currentWorkProfileId));
                if (currentWorkProfileId <= 0)
                    throw new ArgumentOutOfRangeException(nameof(currentWorkProfileId));
                return currentWorkProfileId.Value;
            }
        }

        public int GetCurrentRoleId
        {
            get
            {
                var currentRoleId = _currentUserService.RoleId;
                if (currentRoleId is null)
                    throw new ArgumentNullException(nameof(currentRoleId));
                if (currentRoleId <= 0)
                    throw new ArgumentOutOfRangeException(nameof(currentRoleId));
                return currentRoleId.Value;
            }
        }

        public int GetCurrentCampusId
        {
            get
            {
                var currentCampusId = _currentUserService.CampusId;
                if (currentCampusId is null)
                    throw new ArgumentNullException(nameof(currentCampusId));
                if (currentCampusId <= 0)
                    throw new ArgumentOutOfRangeException(nameof(currentCampusId));
                return currentCampusId.Value;
            }
        }
    }
}