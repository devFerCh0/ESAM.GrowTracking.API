using ESAM.GrowTracking.API.Abstractions.Mappers;
using ESAM.GrowTracking.API.Responses;
using ESAM.GrowTracking.Application.Results;
using Microsoft.AspNetCore.Mvc;

namespace ESAM.GrowTracking.API.Extensions
{
    //public static class ActionResultExtensions
    //{
    //    public static ActionResult ToErrorActionResult(this Result result, IErrorToHttpMapper errorToHttpMapper)
    //    {
    //        ArgumentNullException.ThrowIfNull(result);
    //        ArgumentNullException.ThrowIfNull(errorToHttpMapper);
    //        if (result.IsSuccess)
    //            throw new InvalidOperationException("No se puede construir una respuesta de error a partir de un resultado exitoso.");
    //        var errorsPayload = result.Errors.Select(e => new { message = e.Message, fields = e.Fields }).ToList();
    //        var statusCode = errorToHttpMapper.GetStatusCode(result.Errors.Select(e => e.ErrorType));
    //        return new ObjectResult(new { success = false, errors = errorsPayload }) { StatusCode = statusCode };
    //    }
    //}

    public static class ActionResultExtensions
    {
        public static ActionResult ToErrorActionResult(this Result result, IErrorToHttpMapper errorToHttpMapper, string? traceId = null)
        {
            ArgumentNullException.ThrowIfNull(result);
            ArgumentNullException.ThrowIfNull(errorToHttpMapper);
            if (result.IsSuccess)
                throw new InvalidOperationException("No se puede construir una respuesta de error a partir de un resultado exitoso.");
            var errorsPayload = result.Errors.Select(e => new ApiErrorItem { Message = e.Message, Fields = [.. e.Fields] }).ToList();
            var statusCode = errorToHttpMapper.GetStatusCode(result.Errors.Select(e => e.ErrorType));
            var payload = ApiErrorResponse.From(errorsPayload, traceId);
            return new ObjectResult(payload) { StatusCode = statusCode, ContentTypes = { "application/json" } };
        }
    }
}