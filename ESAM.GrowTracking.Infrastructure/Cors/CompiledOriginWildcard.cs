using System.Text.RegularExpressions;

namespace ESAM.GrowTracking.Infrastructure.Cors
{
    internal sealed class CompiledOriginWildcard
    {
        public required string? Scheme { get; init; }

        public required Regex HostRegex { get; init; }

        public required int? Port { get; init; }
    }
}