using System.Globalization;
using System.Threading.RateLimiting;
using ESAM.GrowTracking.API.Responses;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace ESAM.GrowTracking.API.ConfigureOptions
{
    internal sealed class RateLimiterOptionsSetup : IConfigureOptions<RateLimiterOptions>
    {
        private readonly IHostEnvironment _environment;

        public RateLimiterOptionsSetup(IHostEnvironment environment)
        {
            ArgumentNullException.ThrowIfNull(environment);
            _environment = environment;
        }

        public void Configure(RateLimiterOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            if (_environment.IsDevelopment())
            {
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                    RateLimitPartition.GetNoLimiter(httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown"));
                options.OnRejected = (_, _) => ValueTask.CompletedTask;
                return;
            }
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
            {
                var isAuthEndpoint = httpContext.Request.Path.StartsWithSegments("/api/auth", StringComparison.OrdinalIgnoreCase);
                var clientIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                var partitionKey = isAuthEndpoint ? $"auth:{clientIp}" : $"global:{clientIp}";
                if (isAuthEndpoint)
                    return RateLimitPartition.GetFixedWindowLimiter(partitionKey, _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 10,
                        Window = TimeSpan.FromMinutes(1),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    });
                return RateLimitPartition.GetSlidingWindowLimiter(partitionKey, _ => new SlidingWindowRateLimiterOptions
                {
                    PermitLimit = 100,
                    Window = TimeSpan.FromMinutes(1),
                    SegmentsPerWindow = 6,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 0
                });
            });
            options.OnRejected = async (ctx, _) =>
            {
                if (ctx.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                    ctx.HttpContext.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString(CultureInfo.InvariantCulture);
                await ApiErrorWriter.WriteAsync(ctx.HttpContext, StatusCodes.Status429TooManyRequests,
                    "Has excedido el límite de solicitudes permitidas. Intente de nuevo en un momento.");
            };
        }
    }
}