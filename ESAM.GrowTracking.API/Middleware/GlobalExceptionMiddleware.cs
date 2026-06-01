using ESAM.GrowTracking.API.Responses;
using Microsoft.Extensions.Primitives;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace ESAM.GrowTracking.API.Middleware
{
    //public sealed class GlobalExceptionMiddleware
    //{
    //    private static readonly JsonSerializerOptions s_jsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    //    private static readonly Regex s_sensitivePattern =
    //        new(@"(password|pwd|secret|token|key|bearer|authorization|connectionstring|data\s*source|initial\s*catalog)\s*[=:]\s*\S+",
    //            RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(200));
    //    private static readonly string[] s_corsHeaderNames = [ "Access-Control-Allow-Origin", "Access-Control-Allow-Credentials", "Access-Control-Expose-Headers", "Vary" ];
    //    private readonly RequestDelegate _next;
    //    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    //    private readonly IHostEnvironment _environment;
    //    private readonly ProblemDetailsFactory _problemDetailsFactory;

    //    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger, IHostEnvironment environment, 
    //        ProblemDetailsFactory problemDetailsFactory)
    //    {
    //        ArgumentNullException.ThrowIfNull(next);
    //        ArgumentNullException.ThrowIfNull(logger);
    //        ArgumentNullException.ThrowIfNull(environment);
    //        ArgumentNullException.ThrowIfNull(problemDetailsFactory);
    //        _next = next;
    //        _logger = logger;
    //        _environment = environment;
    //        _problemDetailsFactory = problemDetailsFactory;
    //    }

    //    public async Task Invoke(HttpContext context)
    //    {
    //        ArgumentNullException.ThrowIfNull(context);
    //        try
    //        {
    //            await _next(context);
    //        }
    //        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
    //        {
    //            _logger.LogInformation("La solicitud {TraceId} fue cancelada por el cliente.", context.TraceIdentifier);
    //        }
    //        catch (Exception ex)
    //        {
    //            if (_environment.IsDevelopment())
    //                _logger.LogError(ex, "Excepción no controlada al procesar la solicitud {Method} {Path} (TraceId: {TraceId}).",
    //                    context.Request.Method, context.Request.Path, context.TraceIdentifier);
    //            else
    //                _logger.LogError("Excepción no controlada [{ExceptionType}] al procesar la solicitud {Method} {Path} (TraceId: {TraceId}). Mensaje: {SanitizedMessage}",
    //                    ex.GetType().FullName, context.Request.Method, context.Request.Path, context.TraceIdentifier, SanitizeExceptionMessage(ex.Message));
    //            if (context.Response.HasStarted)
    //            {
    //                _logger.LogWarning("La respuesta ya fue iniciada para TraceId {TraceId}; no se puede escribir la respuesta de error.", context.TraceIdentifier);
    //                return;
    //            }
    //            var preservedCorsHeaders = new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase);
    //            foreach (var name in s_corsHeaderNames)
    //            {
    //                if (context.Response.Headers.TryGetValue(name, out var value))
    //                    preservedCorsHeaders[name] = value;
    //            }
    //            context.Response.Clear();
    //            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
    //            foreach (var (key, value) in preservedCorsHeaders)
    //                context.Response.Headers[key] = value;
    //            var detail = _environment.IsDevelopment() ? ex.ToString() : "Se ha producido un error inesperado.";
    //            var problemDetails = _problemDetailsFactory.CreateProblemDetails(context, StatusCodes.Status500InternalServerError, "Se ha producido un error inesperado.",
    //                "https://tools.ietf.org/html/rfc7231#section-6.6.1", detail);
    //            if (!problemDetails.Extensions.ContainsKey("traceId"))
    //                problemDetails.Extensions["traceId"] = context.TraceIdentifier;
    //            try
    //            {
    //                await context.Response.WriteAsJsonAsync(problemDetails, s_jsonOptions, "application/problem+json; charset=utf-8");
    //            }
    //            catch (Exception writeEx)
    //            {
    //                _logger.LogError(writeEx, "Error al escribir ProblemDetails para TraceId {TraceId}.", context.TraceIdentifier);
    //            }
    //        }
    //    }

    //    private static string SanitizeExceptionMessage(string? message)
    //    {
    //        if (string.IsNullOrWhiteSpace(message))
    //            return string.Empty;
    //        try
    //        {
    //            return s_sensitivePattern.Replace(message, m => $"{m.Groups[1].Value}=[REDACTED]");
    //        }
    //        catch (RegexMatchTimeoutException)
    //        {
    //            return "[tiempo de espera al sanitizar el mensaje]";
    //        }
    //        catch
    //        {
    //            return "[error al sanitizar el mensaje]";
    //        }
    //    }
    //}

    public sealed class GlobalExceptionMiddleware
    {
        private static readonly JsonSerializerOptions s_jsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        private static readonly Regex s_sensitivePattern =
            new(@"(password|pwd|secret|token|key|bearer|authorization|connectionstring|data\s*source|initial\s*catalog)\s*[=:]\s*\S+",
                RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(200));
        private static readonly string[] s_corsHeaderNames = ["Access-Control-Allow-Origin", "Access-Control-Allow-Credentials", "Access-Control-Expose-Headers", "Vary"];
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;
        private readonly IHostEnvironment _environment;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger, IHostEnvironment environment)
        {
            ArgumentNullException.ThrowIfNull(next);
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(environment);
            _next = next;
            _logger = logger;
            _environment = environment;
        }

        public async Task Invoke(HttpContext context)
        {
            ArgumentNullException.ThrowIfNull(context);
            try
            {
                await _next(context);
            }
            catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
            {
                _logger.LogInformation("La solicitud {TraceId} fue cancelada por el cliente.", context.TraceIdentifier);
            }
            catch (Exception ex)
            {
                if (_environment.IsDevelopment())
                    _logger.LogError(ex, "Excepción no controlada al procesar {Method} {Path} (TraceId: {TraceId}).", context.Request.Method, context.Request.Path,
                        context.TraceIdentifier);
                else
                    _logger.LogError("Excepción no controlada [{ExceptionType}] al procesar {Method} {Path} (TraceId: {TraceId}). Mensaje: {SanitizedMessage}",
                        ex.GetType().FullName, context.Request.Method, context.Request.Path, context.TraceIdentifier, SanitizeExceptionMessage(ex.Message));
                if (context.Response.HasStarted)
                {
                    _logger.LogWarning("La respuesta ya fue iniciada para TraceId {TraceId}; no se puede escribir la respuesta de error.", context.TraceIdentifier);
                    return;
                }
                var preservedCorsHeaders = new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase);
                foreach (var name in s_corsHeaderNames)
                    if (context.Response.Headers.TryGetValue(name, out var value))
                        preservedCorsHeaders[name] = value;
                context.Response.Clear();
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                foreach (var (key, value) in preservedCorsHeaders)
                    context.Response.Headers[key] = value;
                var message = _environment.IsDevelopment() ? ex.ToString() : "Se ha producido un error inesperado.";
                var payload = ApiErrorResponse.From([new ApiErrorItem { Message = message, Fields = [] }], traceId: context.TraceIdentifier);
                context.Response.ContentType = "application/json; charset=utf-8";
                await context.Response.WriteAsJsonAsync(payload, s_jsonOptions);
            }
        }

        private static string SanitizeExceptionMessage(string? message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return string.Empty;
            try
            {
                return s_sensitivePattern.Replace(message, m => $"{m.Groups[1].Value}=[REDACTED]");
            }
            catch (RegexMatchTimeoutException)
            {
                return "[tiempo de espera al sanitizar el mensaje]";
            }
            catch
            {
                return "[error al sanitizar el mensaje]";
            }
        }
    }
}