using System.Text.Json.Serialization;

namespace ESAM.GrowTracking.API.Responses
{
    public sealed record ApiErrorResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; init; } = false;

        [JsonPropertyName("source")]
        public string Source { get; init; } = ApiErrorSource.System;

        [JsonPropertyName("errors")]
        public required IReadOnlyList<ApiErrorItem> Errors { get; init; }

        [JsonPropertyName("traceId")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? TraceId { get; init; }

        public static ApiErrorResponse From(IEnumerable<ApiErrorItem> errors, string? traceId = null, string source = ApiErrorSource.System)
        {
            ArgumentNullException.ThrowIfNull(errors);
            var errorArray = errors.ToArray();
            if (errorArray.Length == 0)
                throw new ArgumentException("El payload de error debe contener al menos un elemento.", nameof(errors));
            return new() { Errors = errorArray, TraceId = traceId, Source = source };
        }

        public static ApiErrorResponse From(string message, string? traceId = null, string source = ApiErrorSource.System)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(message);
            return new() { Errors = [new ApiErrorItem { Message = message }], TraceId = traceId, Source = source };
        }
    }
}