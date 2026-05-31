using ESAM.GrowTracking.Infrastructure.Abstractions.Http;
using ESAM.GrowTracking.Infrastructure.Http;
using System.Security.Claims;

namespace ESAM.GrowTracking.API.Adapters
{
    public sealed class HttpContextAdapter(IHttpContextAccessor accessor) : IClaimsPrincipalProvider, IHttpRequestContextReader, IResponseCookieWriter
    {
        private readonly IHttpContextAccessor _accessor = accessor ?? throw new ArgumentNullException(nameof(accessor));

        public ClaimsPrincipal? Current => _accessor.HttpContext?.User;

        public string? GetCookieValue(string cookieName)
        {
            if (string.IsNullOrWhiteSpace(cookieName))
                return null;
            var ctx = _accessor.HttpContext;
            if (ctx is null)
                return null;
            ctx.Request.Cookies.TryGetValue(cookieName, out var value);
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }

        public string? GetHeaderValue(string headerName)
        {
            if (string.IsNullOrWhiteSpace(headerName))
                return null;
            var ctx = _accessor.HttpContext;
            if (ctx is null)
                return null;
            if (ctx.Request.Headers.TryGetValue(headerName, out var values))
            {
                var raw = values.FirstOrDefault();
                return string.IsNullOrWhiteSpace(raw) ? null : raw;
            }
            return null;
        }

        public string? GetRemoteIpAddress()
        {
            var remote = _accessor.HttpContext?.Connection.RemoteIpAddress;
            if (remote is null)
                return null;
            return remote.IsIPv4MappedToIPv6 ? remote.MapToIPv4().ToString() : remote.ToString();
        }

        public string? GetUserAgent() => GetHeaderValue("User-Agent");

        public void Append(string name, string value, ResponseCookieDescriptor options)
        {
            var ctx = _accessor.HttpContext ?? throw new InvalidOperationException("HttpContext no está disponible. No es posible agregar una cookie fuera de una solicitud HTTP activa.");
            ctx.Response.Cookies.Append(name, value, MapCookieOptions(options));
        }

        public void Delete(string name, ResponseCookieDescriptor options)
        {
            var ctx = _accessor.HttpContext ?? throw new InvalidOperationException("HttpContext no está disponible. No es posible eliminar una cookie fuera de una solicitud HTTP activa.");
            ctx.Response.Cookies.Delete(name, MapCookieOptions(options));
        }

        private static CookieOptions MapCookieOptions(ResponseCookieDescriptor descriptor) => new()
        {
            HttpOnly = descriptor.HttpOnly,
            Secure = descriptor.Secure,
            SameSite = MapSameSiteMode(descriptor.SameSite),
            Expires = descriptor.Expires,
            Path = descriptor.Path,
            Domain = descriptor.Domain,
            IsEssential = descriptor.IsEssential
        };

        private static SameSiteMode MapSameSiteMode(SameSitePolicy policy) => policy switch
        {
            SameSitePolicy.None => SameSiteMode.None,
            SameSitePolicy.Lax => SameSiteMode.Lax,
            SameSitePolicy.Strict => SameSiteMode.Strict,
            SameSitePolicy.Unspecified => SameSiteMode.Unspecified,
            _ => SameSiteMode.Lax
        };
    }
}