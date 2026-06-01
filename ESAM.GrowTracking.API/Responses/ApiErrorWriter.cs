using System.Text.Json;

namespace ESAM.GrowTracking.API.Responses
{
    internal static class ApiErrorWriter
    {
        private static readonly JsonSerializerOptions s_jsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        public static Task WriteAsync(HttpContext context, int statusCode, string message, IReadOnlyList<string>? fields = null)
        {
            var payload = ApiErrorResponse.From([new ApiErrorItem { Message = message, Fields = fields ?? [] }], traceId: context.TraceIdentifier);
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json; charset=utf-8";
            return context.Response.WriteAsJsonAsync(payload, s_jsonOptions);
        }
    }
}