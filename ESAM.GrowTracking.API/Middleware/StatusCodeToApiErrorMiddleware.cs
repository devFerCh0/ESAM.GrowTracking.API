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
            var message = status switch
            {
                StatusCodes.Status404NotFound => "Not Found.",
                StatusCodes.Status405MethodNotAllowed => "Method Not Allowed.",
                StatusCodes.Status415UnsupportedMediaType => "Unsupported Media Type.",
                StatusCodes.Status406NotAcceptable => "Not Acceptable.",
                _ when status >= 500 => "Se ha producido un error inesperado.",
                _ => "Request failed."
            };
            await ApiErrorWriter.WriteAsync(context, status, message);
        }
    }
}