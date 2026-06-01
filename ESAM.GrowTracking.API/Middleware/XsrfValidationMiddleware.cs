using ESAM.GrowTracking.API.Responses;
using ESAM.GrowTracking.Infrastructure.Settings;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace ESAM.GrowTracking.API.Middleware
{
    public sealed class XsrfValidationMiddleware
    {
        private static readonly JsonSerializerOptions s_jsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        private readonly RequestDelegate _next;
        private readonly ILogger<XsrfValidationMiddleware> _logger;
        private readonly CookieSettings _cookieSettings;
        private readonly ProblemDetailsFactory _problemDetailsFactory;

        public XsrfValidationMiddleware(RequestDelegate next, ILogger<XsrfValidationMiddleware> logger, IOptions<CookieSettings> cookieSettingsOptions, 
            ProblemDetailsFactory problemDetailsFactory)
        {
            ArgumentNullException.ThrowIfNull(next);
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(cookieSettingsOptions);
            ArgumentNullException.ThrowIfNull(problemDetailsFactory);
            _next = next;
            _logger = logger;
            _cookieSettings = cookieSettingsOptions.Value ?? throw new ArgumentNullException(nameof(cookieSettingsOptions));
            _problemDetailsFactory = problemDetailsFactory;
        }

        public async Task Invoke(HttpContext context)
        {
            ArgumentNullException.ThrowIfNull(context);
            if (IsStateChangingMethod(context.Request.Method) && !IsExemptPath(context.Request.Path)
                && context.Request.Cookies.ContainsKey(_cookieSettings.EffectiveRefreshCookieName()))
            {
                var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                var method = context.Request.Method;
                var path = context.Request.Path.Value ?? string.Empty;
                var traceId = context.TraceIdentifier;
                var xsrfCookieName = _cookieSettings.EffectiveXsrfCookieName();
                if (!context.Request.Headers.TryGetValue("X-XSRF-TOKEN", out var xsrfHeaderValues))
                {
                    _logger.LogWarning("Validación XSRF fallida — encabezado X-XSRF-TOKEN ausente. Method={Method} Path={Path} ClientIp={ClientIp} TraceId={TraceId}",
                        method, path, clientIp, traceId);
                    await DenyAsync(context);
                    return;
                }
                var xsrfHeader = xsrfHeaderValues.FirstOrDefault();
                if (string.IsNullOrWhiteSpace(xsrfHeader))
                {
                    _logger.LogWarning("Validación XSRF fallida — encabezado X-XSRF-TOKEN vacío. Method={Method} Path={Path} ClientIp={ClientIp} TraceId={TraceId}",
                        method, path, clientIp, traceId);
                    await DenyAsync(context);
                    return;
                }
                if (!context.Request.Cookies.TryGetValue(xsrfCookieName, out var xsrfCookieValue) || string.IsNullOrWhiteSpace(xsrfCookieValue))
                {
                    _logger.LogWarning("Validación XSRF fallida — cookie XSRF '{CookieName}' ausente. Method={Method} Path={Path} ClientIp={ClientIp} TraceId={TraceId}",
                        xsrfCookieName, method, path, clientIp, traceId);
                    await DenyAsync(context);
                    return;
                }
                var headerBytes = Encoding.UTF8.GetBytes(xsrfHeader);
                var cookieBytes = Encoding.UTF8.GetBytes(xsrfCookieValue);
                if (!CryptographicOperations.FixedTimeEquals(headerBytes, cookieBytes))
                {
                    _logger.LogWarning("Validación XSRF fallida — los tokens no coinciden (posible intento CSRF). Method={Method} Path={Path} ClientIp={ClientIp} TraceId={TraceId}",
                        method, path, clientIp, traceId);
                    await DenyAsync(context);
                    return;
                }
            }
            await _next(context);
        }

        private bool IsExemptPath(PathString path)
        {
            foreach (var exempt in _cookieSettings.XsrfExemptPaths)
                if (path.StartsWithSegments(exempt, StringComparison.OrdinalIgnoreCase))
                    return true;
            return false;
        }

        //private async Task DenyAsync(HttpContext context)
        //{
        //    context.Response.StatusCode = StatusCodes.Status403Forbidden;
        //    var problemDetails = _problemDetailsFactory.CreateProblemDetails(context, StatusCodes.Status403Forbidden, "Forbidden", 
        //        "https://tools.ietf.org/html/rfc7231#section-6.5.3", "Token XSRF no válido o faltante.");
        //    if (!problemDetails.Extensions.ContainsKey("traceId"))
        //        problemDetails.Extensions["traceId"] = context.TraceIdentifier;
        //    await context.Response.WriteAsJsonAsync(problemDetails, s_jsonOptions, "application/problem+json; charset=utf-8");
        //}

        private static async Task DenyAsync(HttpContext context)
        {
            await ApiErrorWriter.WriteAsync(context, StatusCodes.Status403Forbidden, "Token XSRF no válido o faltante.");
        }

        private static bool IsStateChangingMethod(string method) =>  HttpMethods.IsPost(method) || HttpMethods.IsPut(method) || HttpMethods.IsDelete(method) 
            || HttpMethods.IsPatch(method);
    }
}