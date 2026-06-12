using ESAM.GrowTracking.Application.ValueObjects;
using FluentValidation.Results;

namespace ESAM.GrowTracking.Application.Extensions
{
    public static class CommandFluentValidationExtensions
    {
        public static List<Error> ToCommandErrors(this ValidationResult validationResult)
        {
            ArgumentNullException.ThrowIfNull(validationResult);
            if (validationResult.Errors is null || validationResult.Errors.Count == 0)
                throw new InvalidOperationException("La validación fallida debe contener al menos un error.");
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