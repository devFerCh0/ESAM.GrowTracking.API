using ESAM.GrowTracking.API.Responses;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;

namespace ESAM.GrowTracking.API.Extensions
{
    public static class RequestFluentValidationExtensions
    {
        public static BadRequestObjectResult ToRequestErrorsActionResult(this ValidationResult validationResult, string? traceId = null)
        {
            ArgumentNullException.ThrowIfNull(validationResult);
            if (validationResult.Errors is null || validationResult.Errors.Count == 0)
                throw new InvalidOperationException("La validación fallida debe contener al menos un error.");
            var errorItems = validationResult.Errors.GroupBy(x => x.PropertyName ?? string.Empty).Select(group => new ApiErrorItem
            {
                Message = string.Join("; ", group.Select(x => x.ErrorMessage)),
                Fields = string.IsNullOrWhiteSpace(group.Key) ? [] : [group.Key]
            }).ToList();
            var payload = ApiErrorResponse.From(errorItems, traceId, ApiErrorSource.Request);
            return new BadRequestObjectResult(payload) { ContentTypes = { "application/json" } };
        }
    }
}