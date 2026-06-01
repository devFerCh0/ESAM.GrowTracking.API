using ESAM.GrowTracking.API.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace ESAM.GrowTracking.API.Filters
{
    //public sealed class ValidateModelStateFilter : IActionFilter
    //{
    //    private readonly ProblemDetailsFactory _problemDetailsFactory;
    //    private readonly ILogger<ValidateModelStateFilter> _logger;

    //    public ValidateModelStateFilter(ProblemDetailsFactory problemDetailsFactory, ILogger<ValidateModelStateFilter> logger)
    //    {
    //        ArgumentNullException.ThrowIfNull(problemDetailsFactory);
    //        ArgumentNullException.ThrowIfNull(logger);
    //        _problemDetailsFactory = problemDetailsFactory;
    //        _logger = logger;
    //    }

    //    public void OnActionExecuting(ActionExecutingContext context)
    //    {
    //        ArgumentNullException.ThrowIfNull(context);
    //        if (!context.ModelState.IsValid)
    //        {
    //            var httpContext = context.HttpContext;
    //            var validationProblemDetails = _problemDetailsFactory.CreateValidationProblemDetails(httpContext, context.ModelState, StatusCodes.Status400BadRequest,
    //                "Se produjo uno o más errores de validación.", "https://tools.ietf.org/html/rfc7231#section-6.5.1");
    //            if (!validationProblemDetails.Extensions.ContainsKey("traceId"))
    //                validationProblemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;
    //            var errorDetails = string.Join(" | ", context.ModelState.Where(kvp => kvp.Value is { Errors.Count: > 0 })
    //                .Select(kvp => $"{kvp.Key}: {string.Join(", ", kvp.Value!.Errors.Select(e => e.ErrorMessage))}"));
    //            _logger.LogInformation("Validación de modelo fallida para TraceId {TraceId}. Campos: {Fields}.", httpContext.TraceIdentifier, errorDetails);
    //            context.Result = new ObjectResult(validationProblemDetails)
    //            {
    //                StatusCode = StatusCodes.Status400BadRequest,
    //                DeclaredType = typeof(ValidationProblemDetails),
    //                ContentTypes = { "application/problem+json" }
    //            };
    //        }
    //    }

    //    public void OnActionExecuted(ActionExecutedContext context) { }
    //}

    public sealed class ValidateModelStateFilter : IActionFilter
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
            var httpContext = context.HttpContext;
            var traceId = httpContext.TraceIdentifier;
            var errors = context.ModelState.Where(kvp => kvp.Value is { Errors.Count: > 0 }).Select(kvp =>
            {
                var field = kvp.Key;
                var message = string.Join("; ", kvp.Value!.Errors.Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage) ? "Valor inválido." : e.ErrorMessage));
                return new ApiErrorItem { Message = message, Fields = string.IsNullOrWhiteSpace(field) ? [] : new[] { field } };
            }).ToList();
            var errorDetails = string.Join(" | ", errors.Select(e => $"{string.Join(",", e.Fields)}: {e.Message}"));
            _logger.LogInformation("Validación de modelo fallida para TraceId {TraceId}. Campos: {Fields}.", traceId, errorDetails);
            var payload = ApiErrorResponse.From(errors, traceId);
            context.Result = new BadRequestObjectResult(payload) { ContentTypes = { "application/json" } };
        }
        public void OnActionExecuted(ActionExecutedContext context) { }
    }
}