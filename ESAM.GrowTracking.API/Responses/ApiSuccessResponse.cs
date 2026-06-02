using System.Text.Json.Serialization;

namespace ESAM.GrowTracking.API.Responses
{
    public sealed record ApiSuccessResponse<T>
    {
        [JsonPropertyName("success")]
        public bool Success { get; init; } = true;

        [JsonPropertyName("data")]
        public required T Data { get; init; }

        [JsonPropertyName("traceId")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? TraceId { get; init; }

        public static ApiSuccessResponse<T> From(T data, string? traceId = null) => new() { Data = data, TraceId = traceId };
    }

    public sealed record ApiSuccessResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; init; } = true;

        [JsonPropertyName("traceId")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? TraceId { get; init; }

        public static ApiSuccessResponse From(string? traceId = null) => new() { TraceId = traceId };

        public static readonly ApiSuccessResponse Default = new();
    }
}