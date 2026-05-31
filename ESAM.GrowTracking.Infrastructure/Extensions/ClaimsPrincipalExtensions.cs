using ESAM.GrowTracking.Application.Enums;
using ESAM.GrowTracking.Infrastructure.Security;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;

namespace ESAM.GrowTracking.Infrastructure.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static bool IsAuthenticated(this ClaimsPrincipal user)
        {
            ArgumentNullException.ThrowIfNull(user);
            return user.Identity?.IsAuthenticated ?? false;
        }

        public static AccessTokenType? GetAccessTokenType(this ClaimsPrincipal user)
        {
            ArgumentNullException.ThrowIfNull(user);
            var raw = user.FindFirst(CustomClaimTypes.AccessTokenType)?.Value;
            if (string.IsNullOrWhiteSpace(raw))
                return null;
            if (!byte.TryParse(raw, out var value))
                return null;
            var tokenType = (AccessTokenType)value;
            return Enum.IsDefined(tokenType) ? tokenType : null;
        }

        public static int? GetUserId(this ClaimsPrincipal user)
        {
            ArgumentNullException.ThrowIfNull(user);
            var raw = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            return int.TryParse(raw, out var value) ? value : null;
        }

        public static string? GetSecurityStamp(this ClaimsPrincipal user)
        {
            ArgumentNullException.ThrowIfNull(user);
            var stamp = user.FindFirst(CustomClaimTypes.SecurityStamp)?.Value;
            return string.IsNullOrWhiteSpace(stamp) ? null : stamp;
        }

        public static int? GetTokenVersion(this ClaimsPrincipal user)
        {
            ArgumentNullException.ThrowIfNull(user);
            var raw = user.FindFirst(CustomClaimTypes.TokenVersion)?.Value;
            return int.TryParse(raw, out var value) ? value : null;
        }

        public static string? GetJti(this ClaimsPrincipal user)
        {
            ArgumentNullException.ThrowIfNull(user);
            var jti = user.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
            return string.IsNullOrWhiteSpace(jti) ? null : jti;
        }

        public static DateTime? GetAccessTokenExpiration(this ClaimsPrincipal user)
        {
            ArgumentNullException.ThrowIfNull(user);
            var raw = user.FindFirst(CustomClaimTypes.AccessTokenExpiration)?.Value;
            return long.TryParse(raw, out var unix) ? DateTimeOffset.FromUnixTimeSeconds(unix).UtcDateTime : null;
        }

        public static int? GetUserDeviceId(this ClaimsPrincipal user)
        {
            ArgumentNullException.ThrowIfNull(user);
            var raw = user.FindFirst(CustomClaimTypes.UserDeviceId)?.Value;
            return int.TryParse(raw, out var value) ? value : null;
        }

        public static int? GetUserSessionId(this ClaimsPrincipal user)
        {
            ArgumentNullException.ThrowIfNull(user);
            var raw = user.FindFirst(CustomClaimTypes.UserSessionId)?.Value;
            return int.TryParse(raw, out var value) ? value : null;
        }

        public static bool? GetIsPersistent(this ClaimsPrincipal user)
        {
            ArgumentNullException.ThrowIfNull(user);
            var raw = user.FindFirst(CustomClaimTypes.IsPersistent)?.Value;
            return bool.TryParse(raw, out var value) ? value : null;
        }

        public static int? GetWorkProfileId(this ClaimsPrincipal user)
        {
            ArgumentNullException.ThrowIfNull(user);
            var raw = user.FindFirst(CustomClaimTypes.WorkProfileId)?.Value;
            return int.TryParse(raw, out var value) ? value : null;
        }

        public static int? GetRoleId(this ClaimsPrincipal user)
        {
            ArgumentNullException.ThrowIfNull(user);
            var raw = user.FindFirst(CustomClaimTypes.RoleId)?.Value;
            return int.TryParse(raw, out var value) ? value : null;
        }

        public static int? GetCampusId(this ClaimsPrincipal user)
        {
            ArgumentNullException.ThrowIfNull(user);
            var raw = user.FindFirst(CustomClaimTypes.CampusId)?.Value;
            return int.TryParse(raw, out var value) ? value : null;
        }

        public static List<string> GetPermissions(this ClaimsPrincipal user)
        {
            ArgumentNullException.ThrowIfNull(user);
            return [.. user.FindAll(CustomClaimTypes.Permissions).Select(c => c.Value).Where(v => !string.IsNullOrWhiteSpace(v)).Distinct(StringComparer.OrdinalIgnoreCase)];
        }

        public static bool HasPermission(this ClaimsPrincipal user, string permission)
        {
            ArgumentNullException.ThrowIfNull(user);
            if (string.IsNullOrWhiteSpace(permission))
                throw new ArgumentException($"El {nameof(permission)} proporcionado no puede estar vacío.", nameof(permission));
            return user.FindAll(CustomClaimTypes.Permissions).Any(c => string.Equals(c.Value, permission, StringComparison.OrdinalIgnoreCase));
        }
    }
}