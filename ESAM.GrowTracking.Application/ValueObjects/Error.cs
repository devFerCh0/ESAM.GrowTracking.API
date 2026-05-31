using ESAM.GrowTracking.Application.Enums;

namespace ESAM.GrowTracking.Application.ValueObjects
{
    public sealed class Error : IEquatable<Error>
    {
        private readonly List<string> _fields;
        public string Message { get; }
        public ErrorType ErrorType { get; }
        public IReadOnlyList<string> Fields => _fields;

        private Error(string message, ErrorType errorType, IEnumerable<string>? fields = null)
        {
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException($"El parámetro '{nameof(message)}' no puede ser nulo ni vacío.", nameof(message));
            Message = message;
            ErrorType = errorType;
            _fields = fields?.ToList() ?? [];
        }

        public static Error Validation(string message, params string[] fields) => new(message, ErrorType.Validation, fields);

        public static Error NotFound(string message) => new(message, ErrorType.NotFound);

        public static Error Unauthorized(string message) => new(message, ErrorType.Unauthorized);

        public static Error Forbidden(string message) => new(message, ErrorType.Forbidden);

        public static Error Conflict(string message) => new(message, ErrorType.Conflict);

        public static Error BusinessRule(string message) => new(message, ErrorType.BusinessRule);

        public static Error UnprocessableEntity(string message) => new(message, ErrorType.UnprocessableEntity);

        public static Error Locked(string message) => new(message, ErrorType.Locked);

        public static Error ServerError(string message) => new(message, ErrorType.ServerError);

        public override bool Equals(object? obj) => Equals(obj as Error);

        public bool Equals(Error? other)
        {
            if (other is null)
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return Message == other.Message && ErrorType == other.ErrorType && _fields.SequenceEqual(other._fields);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + Message.GetHashCode();
                hash = hash * 23 + ErrorType.GetHashCode();
                foreach (var field in _fields)
                    hash = hash * 23 + field.GetHashCode();
                return hash;
            }
        }

        public override string ToString()
        {
            var fieldsPart = _fields.Count > 0 ? $" Fields=[{string.Join(",", _fields)}]" : string.Empty;
            return $"Error(Message={Message}, Type={ErrorType}{fieldsPart})";
        }
    }
}