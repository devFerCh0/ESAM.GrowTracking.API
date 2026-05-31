using ESAM.GrowTracking.API.Abstractions.Mappers;
using ESAM.GrowTracking.Application.Enums;

namespace ESAM.GrowTracking.API.Mappers
{
    public sealed class ErrorToHttpMapper : IErrorToHttpMapper
    {
        private static readonly Dictionary<ErrorType, int> s_errorTypeToStatusCodeMap = new()
        {
            { ErrorType.Validation, StatusCodes.Status400BadRequest },
            { ErrorType.BusinessRule, StatusCodes.Status400BadRequest },
            { ErrorType.UnprocessableEntity, StatusCodes.Status422UnprocessableEntity },
            { ErrorType.NotFound, StatusCodes.Status404NotFound },
            { ErrorType.Unauthorized, StatusCodes.Status401Unauthorized },
            { ErrorType.Forbidden, StatusCodes.Status403Forbidden },
            { ErrorType.Conflict, StatusCodes.Status409Conflict },
            { ErrorType.Locked, StatusCodes.Status423Locked },
            { ErrorType.ServerError, StatusCodes.Status500InternalServerError }
        };

        public int GetStatusCode(IEnumerable<ErrorType> errorTypes)
        {
            ArgumentNullException.ThrowIfNull(errorTypes);
            var codes = errorTypes.Select(type => s_errorTypeToStatusCodeMap.TryGetValue(type, out var status) ? status : StatusCodes.Status400BadRequest).ToList();
            if (codes.Count == 0)
                throw new ArgumentException("Debe existir al menos un tipo de error.", nameof(errorTypes));
            return codes.Max();
        }
    }
}