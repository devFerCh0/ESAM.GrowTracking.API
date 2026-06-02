using System.Text.Json;

namespace ESAM.GrowTracking.API.Responses
{
    internal static class ApiErrorWriter
    {
        private static readonly JsonSerializerOptions s_jsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        public static Task WriteAsync(HttpContext context, int statusCode, IReadOnlyList<ApiErrorItem> errors)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(errors);
            if (errors.Count == 0)
                throw new ArgumentException("Debe existir al menos un error.", nameof(errors));
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json; charset=utf-8";
            var payload = ApiErrorResponse.From(errors, context.TraceIdentifier);
            return context.Response.WriteAsJsonAsync(payload, s_jsonOptions);
        }

        public static Task WriteAsync(HttpContext context, int statusCode, string message, IReadOnlyList<string>? fields = null)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentException.ThrowIfNullOrWhiteSpace(message);
            return WriteAsync(context, statusCode, [new ApiErrorItem { Message = message, Fields = fields ?? [] }]);
        }
    }
}