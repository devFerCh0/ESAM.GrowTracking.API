using ESAM.GrowTracking.API.Responses;
using ESAM.GrowTracking.Infrastructure.Settings;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace ESAM.GrowTracking.API.Middleware
{
    public sealed class XsrfValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<XsrfValidationMiddleware> _logger;
        private readonly CookieSettings _cookieSettings;

        public XsrfValidationMiddleware(RequestDelegate next, ILogger<XsrfValidationMiddleware> logger, IOptions<CookieSettings> cookieSettingsOptions)
        {
            ArgumentNullException.ThrowIfNull(next);
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(cookieSettingsOptions);
            _next = next;
            _logger = logger;
            _cookieSettings = cookieSettingsOptions.Value ?? throw new ArgumentNullException(nameof(cookieSettingsOptions));
        }

        public async Task Invoke(HttpContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            if (IsStateChangingMethod(context.Request.Method) && !IsExemptPath(context.Request.Path)
                && context.Request.Cookies.ContainsKey(_cookieSettings.EffectiveRefreshCookieName()))
            {
                if (HasBearerToken(context.Request))
                {
                    await _next(context);
                    return;
                }
                var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                var method = context.Request.Method;
                var path = context.Request.Path.Value ?? string.Empty;
                var traceId = context.TraceIdentifier;
                var xsrfCookieName = _cookieSettings.EffectiveXsrfCookieName();
                if (!context.Request.Headers.TryGetValue("X-XSRF-TOKEN", out var xsrfHeaderValues))
                {
                    _logger.LogWarning("XsrfValidationMiddleware: encabezado X-XSRF-TOKEN ausente. Method={Method} Path={Path} ClientIp={ClientIp} TraceId={TraceId}", method, path, 
                        clientIp, traceId);
                    await DenyAsync(context);
                    return;
                }
                var xsrfHeader = xsrfHeaderValues.FirstOrDefault();
                if (string.IsNullOrWhiteSpace(xsrfHeader))
                {
                    _logger.LogWarning("XsrfValidationMiddleware: encabezado X-XSRF-TOKEN vacío. Method={Method} Path={Path} ClientIp={ClientIp} TraceId={TraceId}", method, path, 
                        clientIp, traceId);
                    await DenyAsync(context);
                    return;
                }
                if (!context.Request.Cookies.TryGetValue(xsrfCookieName, out var xsrfCookieValue) || string.IsNullOrWhiteSpace(xsrfCookieValue))
                {
                    _logger.LogWarning("XsrfValidationMiddleware: cookie XSRF '{CookieName}' ausente. Method={Method} Path={Path} ClientIp={ClientIp} TraceId={TraceId}", 
                        xsrfCookieName, method, path, clientIp, traceId);
                    await DenyAsync(context);
                    return;
                }
                var headerBytes = Encoding.UTF8.GetBytes(xsrfHeader);
                var cookieBytes = Encoding.UTF8.GetBytes(xsrfCookieValue);
                if (!CryptographicOperations.FixedTimeEquals(headerBytes, cookieBytes))
                {
                    _logger.LogWarning("XsrfValidationMiddleware: tokens no coinciden (posible CSRF). Method={Method} Path={Path} ClientIp={ClientIp} TraceId={TraceId}", method, 
                        path, clientIp, traceId);
                    await DenyAsync(context);
                    return;
                }
            }
            await _next(context);
        }

        private static bool HasBearerToken(HttpRequest request)
        {
            var authHeader = request.Headers.Authorization.FirstOrDefault();
            return authHeader is { Length: > 7 }&& authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(authHeader[7..]);
        }

        private bool IsExemptPath(PathString path)
        {
            foreach (var exempt in _cookieSettings.XsrfExemptPaths)
                if (path.StartsWithSegments(exempt, StringComparison.OrdinalIgnoreCase))
                    return true;
            return false;
        }

        private static Task DenyAsync(HttpContext context) => ApiErrorWriter.WriteAsync(context, StatusCodes.Status403Forbidden, "Token XSRF no válido o faltante.");

        private static bool IsStateChangingMethod(string method) =>
            HttpMethods.IsPost(method) || HttpMethods.IsPut(method) || HttpMethods.IsDelete(method) || HttpMethods.IsPatch(method);
    }
}