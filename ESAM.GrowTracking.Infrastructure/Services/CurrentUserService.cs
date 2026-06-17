using ESAM.GrowTracking.Application.Abstractions.Services;
using ESAM.GrowTracking.Application.Enums;
using ESAM.GrowTracking.Domain.Enums;
using ESAM.GrowTracking.Infrastructure.Abstractions.Http;
using ESAM.GrowTracking.Infrastructure.Extensions;
using System.Security.Claims;

namespace ESAM.GrowTracking.Infrastructure.Services
{
    public sealed class CurrentUserService : ICurrentUserService
    {
        private readonly ClaimsPrincipal? _user;
        private readonly Lazy<HashSet<string>> _permissionsSet;
        private readonly Lazy<IReadOnlyList<string>> _permissionsList;

        public CurrentUserService(IClaimsPrincipalProvider principalProvider)
        {
            ArgumentNullException.ThrowIfNull(principalProvider);
            _user = principalProvider.Current;
            _permissionsSet = new Lazy<HashSet<string>>(() => new HashSet<string>(_user?.GetPermissions() ?? [], StringComparer.OrdinalIgnoreCase), isThreadSafe: true);
            _permissionsList = new Lazy<IReadOnlyList<string>>(() => _permissionsSet.Value.ToList().AsReadOnly(), isThreadSafe: true);
        }

        public bool IsAuthenticated => _user?.IsAuthenticated() ?? false;

        public AccessTokenType? AccessTokenType => _user?.GetAccessTokenType();

        public int? UserId => _user?.GetUserId();

        public string? SecurityStamp => _user?.GetSecurityStamp();

        public int? TokenVersion => _user?.GetTokenVersion();

        public string? Jti => _user?.GetJti();

        public DateTime? AccessTokenExpiration => _user?.GetAccessTokenExpiration();

        public int? UserDeviceId => _user?.GetUserDeviceId();

        public int? UserSessionId => _user?.GetUserSessionId();

        public bool? IsPersistent => _user?.GetIsPersistent();

        public int? WorkProfileId => _user?.GetWorkProfileId();

        public WorkProfileType? WorkProfileType => _user?.GetWorkProfileType();

        public int? RoleId => _user?.GetRoleId();

        public int? CampusId => _user?.GetCampusId();

        public IReadOnlyList<string> Permissions => _permissionsList.Value;

        public bool HasPermission(string permission)
        {
            if (string.IsNullOrWhiteSpace(permission))
                throw new ArgumentException($"{nameof(permission)} no puede ser vacío.", nameof(permission));
            return _permissionsSet.Value.Contains(permission);
        }
    }
}