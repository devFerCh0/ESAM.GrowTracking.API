using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ESAM.GrowTracking.API.HealthChecks
{
    internal static class HealthCheckResponseWriters
    {
        public static Task WriteLiveResponse(HttpContext context, HealthReport _)
        {
            context.Response.ContentType = "application/json";
            return context.Response.WriteAsJsonAsync(new { status = "Healthy" });
        }

        public static async Task WriteReadyResponse(HttpContext context, HealthReport report)
        {
            context.Response.ContentType = "application/json";
            var result = new
            {
                status = report.Status.ToString(),
                checks = report.Entries.Select(e => new { key = e.Key, status = e.Value.Status.ToString(), description = e.Value.Description })
            };
            await context.Response.WriteAsJsonAsync(result);
        }
    }
}