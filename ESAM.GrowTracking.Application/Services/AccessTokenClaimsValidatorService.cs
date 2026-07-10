using ESAM.GrowTracking.Application.Abstractions.Services;
using ESAM.GrowTracking.Application.DTOs;
using ESAM.GrowTracking.Application.Enums;
using ESAM.GrowTracking.Domain.Enums;

namespace ESAM.GrowTracking.Application.Services
{
    public class AccessTokenClaimsValidatorService : IAccessTokenClaimsValidatorService
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly ITokenService _tokenService;
        private AccessTokenClaimsDTO? _overriddenClaims;

        public AccessTokenClaimsValidatorService(ICurrentUserService currentUserService, ITokenService tokenService)
        {
            ArgumentNullException.ThrowIfNull(currentUserService);
            ArgumentNullException.ThrowIfNull(tokenService);
            _currentUserService = currentUserService;
            _tokenService = tokenService;
        }

        public bool IsAuthenticated => _currentUserService.IsAuthenticated;

        public AccessTokenType CurrentAccessTokenType
        {
            get
            {
                var currentAccessTokenType = _overriddenClaims?.AccessTokenType ?? _currentUserService.AccessTokenType;
                if (currentAccessTokenType is null)
                    throw new ArgumentNullException(nameof(currentAccessTokenType));
                return currentAccessTokenType.Value;
            }
        }

        public int CurrentUserId
        {
            get
            {
                var currentUserId = _overriddenClaims?.UserId ?? _currentUserService.UserId;
                if (currentUserId is null)
                    throw new ArgumentNullException(nameof(currentUserId));
                if (currentUserId <= 0)
                    throw new ArgumentOutOfRangeException(nameof(currentUserId));
                return currentUserId.Value;
            }
        }

        public string CurrentSecurityStamp
        {
            get
            {
                var currentSecurityStamp = _overriddenClaims?.SecurityStamp ?? _currentUserService.SecurityStamp;
                if (string.IsNullOrWhiteSpace(currentSecurityStamp))
                    throw new ArgumentException(nameof(currentSecurityStamp));
                return currentSecurityStamp;
            }
        }

        public int CurrentTokenVersion
        {
            get
            {
                var currentTokenVersion = _overriddenClaims?.TokenVersion ?? _currentUserService.TokenVersion;
                if (currentTokenVersion is null)
                    throw new ArgumentNullException(nameof(currentTokenVersion));
                if (currentTokenVersion < 0)
                    throw new ArgumentOutOfRangeException(nameof(currentTokenVersion));
                return currentTokenVersion.Value;
            }
        }

        public string CurrentJti
        {
            get
            {
                var currentJti = _overriddenClaims?.Jti ?? _currentUserService.Jti;
                if (string.IsNullOrWhiteSpace(currentJti))
                    throw new ArgumentException(nameof(currentJti));
                return currentJti;
            }
        }

        public DateTime CurrentAccessTokenExpiration
        {
            get
            {
                var currentAccessTokenExpiration = _overriddenClaims?.AccessTokenExpiration ?? _currentUserService.AccessTokenExpiration;
                if (currentAccessTokenExpiration is null)
                    throw new ArgumentException(nameof(currentAccessTokenExpiration));
                return currentAccessTokenExpiration.Value;
            }
        }

        public int CurrentUserDeviceId
        {
            get
            {
                var currentUserDeviceId = _overriddenClaims?.UserDeviceId ?? _currentUserService.UserDeviceId;
                if (currentUserDeviceId is null)
                    throw new ArgumentNullException(nameof(currentUserDeviceId));
                if (currentUserDeviceId <= 0)
                    throw new ArgumentOutOfRangeException(nameof(currentUserDeviceId));
                return currentUserDeviceId.Value;
            }
        }

        public int CurrentUserSessionId
        {
            get
            {
                var currentUserSessionId = _overriddenClaims?.UserSessionId ?? _currentUserService.UserSessionId;
                if (currentUserSessionId is null)
                    throw new ArgumentNullException(nameof(currentUserSessionId));
                if (currentUserSessionId <= 0)
                    throw new ArgumentOutOfRangeException(nameof(currentUserSessionId));
                return currentUserSessionId.Value;
            }
        }

        public bool CurrentIsPersistent
        {
            get
            {
                var currentIsPersistent = _currentUserService.IsPersistent;
                if (currentIsPersistent is null)
                    throw new ArgumentException(nameof(currentIsPersistent));
                return currentIsPersistent.Value;
            }
        }

        public int CurrentWorkProfileSelectedId
        {
            get
            {
                var currentWorkProfileSelectedId = _overriddenClaims?.WorkProfileSelectedId ?? _currentUserService.WorkProfileSelectedId;
                if (currentWorkProfileSelectedId is null)
                    throw new ArgumentNullException(nameof(currentWorkProfileSelectedId));
                if (currentWorkProfileSelectedId <= 0)
                    throw new ArgumentOutOfRangeException(nameof(currentWorkProfileSelectedId));
                return currentWorkProfileSelectedId.Value;
            }
        }

        public int CurrentWorkProfileId
        {
            get
            {
                var currentWorkProfileId = _overriddenClaims?.WorkProfileId ?? _currentUserService.WorkProfileId;
                if (currentWorkProfileId is null)
                    throw new ArgumentNullException(nameof(currentWorkProfileId));
                if (currentWorkProfileId <= 0)
                    throw new ArgumentOutOfRangeException(nameof(currentWorkProfileId));
                return currentWorkProfileId.Value;
            }
        }

        public WorkProfileType CurrentWorkProfileType
        {
            get
            {
                var currentWorkProfileType = _overriddenClaims?.WorkProfileType ?? _currentUserService.WorkProfileType;
                if (currentWorkProfileType is null)
                    throw new ArgumentNullException(nameof(currentWorkProfileType));
                return currentWorkProfileType.Value;
            }
        }

        public int CurrentRoleCampusSelectedId
        {
            get
            {
                var currentRoleCampusSelectedId = _overriddenClaims?.RoleCampusSelectedId ?? _currentUserService.RoleCampusSelectedId;
                if (currentRoleCampusSelectedId is null)
                    throw new ArgumentNullException(nameof(currentRoleCampusSelectedId));
                if (currentRoleCampusSelectedId <= 0)
                    throw new ArgumentOutOfRangeException(nameof(currentRoleCampusSelectedId));
                return currentRoleCampusSelectedId.Value;
            }
        }

        public int CurrentRoleId
        {
            get
            {
                var currentRoleId = _overriddenClaims?.RoleId ?? _currentUserService.RoleId;
                if (currentRoleId is null)
                    throw new ArgumentNullException(nameof(currentRoleId));
                if (currentRoleId <= 0)
                    throw new ArgumentOutOfRangeException(nameof(currentRoleId));
                return currentRoleId.Value;
            }
        }

        public int CurrentCampusId
        {
            get
            {
                var currentCampusId = _overriddenClaims?.CampusId ?? _currentUserService.CampusId;
                if (currentCampusId is null)
                    throw new ArgumentNullException(nameof(currentCampusId));
                if (currentCampusId <= 0)
                    throw new ArgumentOutOfRangeException(nameof(currentCampusId));
                return currentCampusId.Value;
            }
        }

        public async Task UseExplicitAccessTokenAsync(string accessToken)
        {
            _overriddenClaims = await _tokenService.ExtractAccessTokenClaimsAsync(accessToken);
        }
    }

    //public class AccessTokenClaimsValidatorService : IAccessTokenClaimsValidatorService
    //{
    //    private readonly ICurrentUserService _currentUserService;
    //    private readonly ITokenService _tokenService;
    //    private AccessTokenClaimsDTO? _overriddenClaims;

    //    public AccessTokenClaimsValidatorService(ICurrentUserService currentUserService, ITokenService tokenService)
    //    {
    //        ArgumentNullException.ThrowIfNull(currentUserService);
    //        ArgumentNullException.ThrowIfNull(tokenService);
    //        _currentUserService = currentUserService;
    //        _tokenService = tokenService;
    //    }

    //    public bool IsAuthenticated => _currentUserService.IsAuthenticated;

    //    public AccessTokenType CurrentAccessTokenType
    //    {
    //        get
    //        {
    //            var currentAccessTokenType = _overriddenClaims?.AccessTokenType ?? _currentUserService.AccessTokenType;
    //            if (currentAccessTokenType is null)
    //                throw new ArgumentNullException(nameof(currentAccessTokenType));
    //            return currentAccessTokenType.Value;
    //        }
    //    }

    //    public int CurrentUserId
    //    {
    //        get
    //        {
    //            var currentUserId = _overriddenClaims?.UserId ?? _currentUserService.UserId;
    //            if (currentUserId is null)
    //                throw new ArgumentNullException(nameof(currentUserId));
    //            if (currentUserId <= 0)
    //                throw new ArgumentOutOfRangeException(nameof(currentUserId));
    //            return currentUserId.Value;
    //        }
    //    }

    //    public string CurrentSecurityStamp
    //    {
    //        get
    //        {
    //            var currentSecurityStamp = _overriddenClaims?.SecurityStamp ?? _currentUserService.SecurityStamp;
    //            if (string.IsNullOrWhiteSpace(currentSecurityStamp))
    //                throw new ArgumentException(nameof(currentSecurityStamp));
    //            return currentSecurityStamp;
    //        }
    //    }

    //    public int CurrentTokenVersion
    //    {
    //        get
    //        {
    //            var currentTokenVersion = _overriddenClaims?.TokenVersion ?? _currentUserService.TokenVersion;
    //            if (currentTokenVersion is null)
    //                throw new ArgumentNullException(nameof(currentTokenVersion));
    //            if (currentTokenVersion < 0)
    //                throw new ArgumentOutOfRangeException(nameof(currentTokenVersion));
    //            return currentTokenVersion.Value;
    //        }
    //    }

    //    public string CurrentJti
    //    {
    //        get
    //        {
    //            var currentJti = _overriddenClaims?.Jti ?? _currentUserService.Jti;
    //            if (string.IsNullOrWhiteSpace(currentJti))
    //                throw new ArgumentException(nameof(currentJti));
    //            return currentJti;
    //        }
    //    }

    //    public DateTime CurrentAccessTokenExpiration
    //    {
    //        get
    //        {
    //            var currentAccessTokenExpiration = _overriddenClaims?.AccessTokenExpiration ?? _currentUserService.AccessTokenExpiration;
    //            if (currentAccessTokenExpiration is null)
    //                throw new ArgumentException(nameof(currentAccessTokenExpiration));
    //            return currentAccessTokenExpiration.Value;
    //        }
    //    }

    //    public int CurrentUserDeviceId
    //    {
    //        get
    //        {
    //            var currentUserDeviceId = _overriddenClaims?.UserDeviceId ?? _currentUserService.UserDeviceId;
    //            if (currentUserDeviceId is null)
    //                throw new ArgumentNullException(nameof(currentUserDeviceId));
    //            if (currentUserDeviceId <= 0)
    //                throw new ArgumentOutOfRangeException(nameof(currentUserDeviceId));
    //            return currentUserDeviceId.Value;
    //        }
    //    }

    //    public int CurrentUserSessionId
    //    {
    //        get
    //        {
    //            var currentUserSessionId = _overriddenClaims?.UserSessionId ?? _currentUserService.UserSessionId;
    //            if (currentUserSessionId is null)
    //                throw new ArgumentNullException(nameof(currentUserSessionId));
    //            if (currentUserSessionId <= 0)
    //                throw new ArgumentOutOfRangeException(nameof(currentUserSessionId));
    //            return currentUserSessionId.Value;
    //        }
    //    }

    //    public bool CurrentIsPersistent
    //    {
    //        get
    //        {
    //            var currentIsPersistent = _currentUserService.IsPersistent;
    //            if (currentIsPersistent is null)
    //                throw new ArgumentException(nameof(currentIsPersistent));
    //            return currentIsPersistent.Value;
    //        }
    //    }

    //    public int CurrentWorkProfileId
    //    {
    //        get
    //        {
    //            var currentWorkProfileId = _overriddenClaims?.WorkProfileId ?? _currentUserService.WorkProfileId;
    //            if (currentWorkProfileId is null)
    //                throw new ArgumentNullException(nameof(currentWorkProfileId));
    //            if (currentWorkProfileId <= 0)
    //                throw new ArgumentOutOfRangeException(nameof(currentWorkProfileId));
    //            return currentWorkProfileId.Value;
    //        }
    //    }

    //    public WorkProfileType CurrentWorkProfileType
    //    {
    //        get
    //        {
    //            var currentWorkProfileType = _overriddenClaims?.WorkProfileType ?? _currentUserService.WorkProfileType;
    //            if (currentWorkProfileType is null)
    //                throw new ArgumentNullException(nameof(currentWorkProfileType));
    //            return currentWorkProfileType.Value;
    //        }
    //    }

    //    public int CurrentRoleId
    //    {
    //        get
    //        {
    //            var currentRoleId = _overriddenClaims?.RoleId ?? _currentUserService.RoleId;
    //            if (currentRoleId is null)
    //                throw new ArgumentNullException(nameof(currentRoleId));
    //            if (currentRoleId <= 0)
    //                throw new ArgumentOutOfRangeException(nameof(currentRoleId));
    //            return currentRoleId.Value;
    //        }
    //    }

    //    public int CurrentCampusId
    //    {
    //        get
    //        {
    //            var currentCampusId = _overriddenClaims?.CampusId ?? _currentUserService.CampusId;
    //            if (currentCampusId is null)
    //                throw new ArgumentNullException(nameof(currentCampusId));
    //            if (currentCampusId <= 0)
    //                throw new ArgumentOutOfRangeException(nameof(currentCampusId));
    //            return currentCampusId.Value;
    //        }
    //    }

    //    public async Task UseExplicitAccessTokenAsync(string accessToken)
    //    {
    //        _overriddenClaims = await _tokenService.ExtractAccessTokenClaimsAsync(accessToken);
    //    }
    //}
}