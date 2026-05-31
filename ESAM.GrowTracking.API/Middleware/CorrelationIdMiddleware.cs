using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Primitives;

namespace ESAM.GrowTracking.API.Middleware
{
    public sealed class CorrelationIdMiddleware
    {
        public const string HeaderName = "X-Correlation-ID";
        private const int MaxCorrelationIdLength = 128;
        private readonly RequestDelegate _next;
        private readonly ILogger<CorrelationIdMiddleware> _logger;

        public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
        {
            ArgumentNullException.ThrowIfNull(next);
            ArgumentNullException.ThrowIfNull(logger);
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            ArgumentNullException.ThrowIfNull(context);
            var correlationId = ResolveCorrelationId(context);
            var identifierFeature = context.Features.Get<IHttpRequestIdentifierFeature>();
            if (identifierFeature is not null)
                identifierFeature.TraceIdentifier = correlationId;
            context.Items[HeaderName] = correlationId;
            context.Response.OnStarting(() =>
            {
                if (!context.Response.Headers.ContainsKey(HeaderName))
                    context.Response.Headers[HeaderName] = correlationId;
                return Task.CompletedTask;
            });
            using (_logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = correlationId }))
            {
                await _next(context);
            }
        }

        private static string ResolveCorrelationId(HttpContext context)
        {
            if (context.Request.Headers.TryGetValue(HeaderName, out var incoming) && !StringValues.IsNullOrEmpty(incoming))
            {
                var value = incoming[0]?.Trim();
                if (!string.IsNullOrWhiteSpace(value) && value.Length <= MaxCorrelationIdLength)
                    return value;
            }
            return context.TraceIdentifier;
        }
    }
}