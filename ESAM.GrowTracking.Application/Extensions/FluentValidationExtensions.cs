using ESAM.GrowTracking.Application.ValueObjects;
using FluentValidation.Results;

namespace ESAM.GrowTracking.Application.Extensions
{
    public static class FluentValidationExtensions
    {
        public static List<Error> ToDomainErrors(this ValidationResult validationResult)
        {
            ArgumentNullException.ThrowIfNull(validationResult);
            return [.. validationResult.Errors.GroupBy(vf => vf.PropertyName ?? string.Empty).Select(group => 
            {
                var message = string.Join("; ", group.Select(f => f.ErrorMessage));
                var propertyName = group.Key;
                if (string.IsNullOrWhiteSpace(propertyName))
                    return Error.Validation(message);
                return Error.Validation(message, propertyName);
            })];
        }
    }
}