using ESAM.GrowTracking.Application.Utilities;
using System.Text.RegularExpressions;

namespace ESAM.GrowTracking.Application.Validations
{
    internal static partial class ValidationRules
    {
        [GeneratedRegex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", RegexOptions.CultureInvariant)]
        private static partial Regex EmailRegex();

        [GeneratedRegex(@"^[a-zA-Z0-9_.-]+$", RegexOptions.CultureInvariant)]
        private static partial Regex UsernameRegex();

        [GeneratedRegex(@"^[a-zA-Z0-9\-_:.]{3,128}$", RegexOptions.CultureInvariant)]
        private static partial Regex DeviceIdentifierRegex();

        public static bool IsValidCredential(string? credential)
        {
            if (string.IsNullOrWhiteSpace(credential))
                return false;
            var cleanCredential = credential.Trim();
            return cleanCredential.Contains('@') ? EmailRegex().IsMatch(cleanCredential) : UsernameRegex().IsMatch(cleanCredential);
        }

        public static bool IsValidGuid(string? deviceId)
        {
            if (string.IsNullOrWhiteSpace(deviceId))
                return false;
            var clean = deviceId.Trim();
            return Guid.TryParse(clean, out _) || DeviceIdentifierRegex().IsMatch(clean);
        }

        public static bool HasNoControlChars(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;
            return !value.Any(char.IsControl);
        }

        public static bool BeAValidEnum<TEnum>(string? value) where TEnum : struct, Enum
        {
            return EnumHelper.TryParseFromString<TEnum>(value, out _);
        }
    }
}