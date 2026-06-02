using ESAM.GrowTracking.API.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ESAM.GrowTracking.API.Filters
{
    public sealed class ValidateModelStateFilter : IActionFilter // 1, 2
    {
        private readonly ILogger<ValidateModelStateFilter> _logger;

        public ValidateModelStateFilter(ILogger<ValidateModelStateFilter> logger)
        {
            ArgumentNullException.ThrowIfNull(logger);
            _logger = logger;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            ArgumentNullException.ThrowIfNull(context);
            if (context.ModelState.IsValid)
                return;
            var traceId = context.HttpContext.TraceIdentifier;
            var errors = context.ModelState.Where(kvp => kvp.Value is { Errors.Count: > 0 }).Select(kvp =>
            {
                var field = kvp.Key;
                var message = string.Join("; ", kvp.Value!.Errors.Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage) ? "Valor inválido." : e.ErrorMessage));
                return new ApiErrorItem { Message = message, Fields = string.IsNullOrWhiteSpace(field) ? [] : [field] };
            }).ToList();
            _logger.LogWarning("ValidateModelStateFilter: modelo inválido. TraceId={TraceId}. Errores: {Errors}", traceId,
                string.Join(" | ", errors.Select(e => $"[{string.Join(",", e.Fields)}]: {e.Message}")));
            var payload = ApiErrorResponse.From(errors, traceId);
            context.Result = new BadRequestObjectResult(payload) { ContentTypes = { "application/json" } };
        }

        public void OnActionExecuted(ActionExecutedContext context) { }
    }
}