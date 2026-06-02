using ESAM.GrowTracking.API.Responses;

namespace ESAM.GrowTracking.API.Middleware
{
    public sealed class StatusCodeToApiErrorMiddleware
    {
        private readonly RequestDelegate _next;

        public StatusCodeToApiErrorMiddleware(RequestDelegate next)
        {
            ArgumentNullException.ThrowIfNull(next);
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            await _next(context);
            if (context.Response.HasStarted)
                return;
            if (context.Response.StatusCode is StatusCodes.Status204NoContent or StatusCodes.Status304NotModified)
                return;
            if (!string.IsNullOrWhiteSpace(context.Response.ContentType))
                return;
            var status = context.Response.StatusCode;
            if (status < 400)
                return;
            await ApiErrorWriter.WriteAsync(context, status, ResolveMessage(status));
        }

        private static string ResolveMessage(int statusCode) => statusCode switch
        {
            StatusCodes.Status400BadRequest => "Bad Request.",
            StatusCodes.Status401Unauthorized => "Unauthorized.",
            StatusCodes.Status403Forbidden => "Forbidden.",
            StatusCodes.Status404NotFound => "Not Found.",
            StatusCodes.Status405MethodNotAllowed => "Method Not Allowed.",
            StatusCodes.Status406NotAcceptable => "Not Acceptable.",
            StatusCodes.Status408RequestTimeout => "Request Timeout.",
            StatusCodes.Status409Conflict => "Conflict.",
            StatusCodes.Status410Gone => "Gone.",
            StatusCodes.Status413RequestEntityTooLarge => "Request Entity Too Large.",
            StatusCodes.Status415UnsupportedMediaType => "Unsupported Media Type.",
            StatusCodes.Status422UnprocessableEntity => "Unprocessable Entity.",
            StatusCodes.Status423Locked => "Locked.",
            StatusCodes.Status429TooManyRequests => "Too Many Requests.",
            StatusCodes.Status431RequestHeaderFieldsTooLarge => "Request Header Fields Too Large.",
            >= 500 => "Se ha producido un error inesperado.",
            _ => "Request failed."
        };
    }
}