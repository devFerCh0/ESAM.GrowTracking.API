using System.Text.RegularExpressions;

namespace ESAM.GrowTracking.Application.Validations
{
    public static partial class CommandValidationRules
    {
        [GeneratedRegex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", RegexOptions.CultureInvariant)]
        private static partial Regex EmailRegex();

        [GeneratedRegex(@"^[a-zA-Z0-9_.-]+$", RegexOptions.CultureInvariant)]
        private static partial Regex UsernameRegex();

        [GeneratedRegex(@"^[a-zA-Z0-9\-_:.]{3,256}$", RegexOptions.CultureInvariant)]
        private static partial Regex DeviceIdentifierRegex();

        public static bool IsValidCredential(string credential)
        {
            var cleanCredential = credential.Trim();
            return cleanCredential.Contains('@') ? EmailRegex().IsMatch(cleanCredential) : UsernameRegex().IsMatch(cleanCredential);
        }

        public static bool IsValidGuid(string deviceId)
        {
            var clean = deviceId.Trim();
            return Guid.TryParse(clean, out _) || DeviceIdentifierRegex().IsMatch(clean);
        }

        public static bool HasNoControlChars(string value)
        {
            return !value.Any(char.IsControl);
        }
    }
}