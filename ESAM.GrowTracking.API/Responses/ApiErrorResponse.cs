using System.Text.Json.Serialization;

namespace ESAM.GrowTracking.API.Responses
{
    public sealed record ApiErrorResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; init; } = false;

        [JsonPropertyName("errors")]
        public required IReadOnlyList<ApiErrorItem> Errors { get; init; }

        [JsonPropertyName("traceId")]
        public string? TraceId { get; init; }

        public static ApiErrorResponse From(IEnumerable<ApiErrorItem> errors, string? traceId = null) => new() { Errors = [.. errors], TraceId = traceId };
    }
}