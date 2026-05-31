namespace ESAM.GrowTracking.Infrastructure.Http
{
    public sealed class ResponseCookieDescriptor
    {
        public bool HttpOnly { get; init; }

        public bool Secure { get; init; }

        public SameSitePolicy SameSite { get; init; } = SameSitePolicy.Lax;

        public DateTimeOffset? Expires { get; init; }

        public string Path { get; init; } = "/";

        public string? Domain { get; init; }

        public bool IsEssential { get; init; }
    }
}