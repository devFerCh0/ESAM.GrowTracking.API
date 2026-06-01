using System.Text.Json.Serialization;

namespace ESAM.GrowTracking.API.Responses
{
    public sealed record ApiErrorItem
    {
        [JsonPropertyName("message")]
        public required string Message { get; init; }

        [JsonPropertyName("fields")]
        public IReadOnlyList<string> Fields { get; init; } = [];

        [JsonPropertyName("type")]
        public string? Type { get; init; }
    }
}