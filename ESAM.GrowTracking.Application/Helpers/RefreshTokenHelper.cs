namespace ESAM.GrowTracking.Application.Helpers
{
    internal static class RefreshTokenParser
    {
        internal static void TryParse(string? raw, out string? identifier, out string? tokenPlain)
        {
            identifier = null;
            tokenPlain = null;
            if (string.IsNullOrWhiteSpace(raw))
                return;
            var parts = raw.Split('.');
            if (parts.Length != 2)
                return;
            identifier = parts[0];
            tokenPlain = parts[1];
        }
    }
}