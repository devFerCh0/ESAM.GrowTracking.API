using System.Globalization;

namespace ESAM.GrowTracking.Application.Utilities
{
    public static class EnumHelper
    {
        public static string GetStringValue<TEnum>(this TEnum enumValue) where TEnum : struct, Enum
        {
            var numericValue = Convert.ToUInt64(enumValue);
            if (EnumMetadata<TEnum>.IsFlags && numericValue != 0UL)
            {
                var pieces = new List<string>();
                foreach (var (fieldNumeric, fieldString, _) in EnumMetadata<TEnum>.Fields)
                    if (fieldNumeric != 0UL && (numericValue & fieldNumeric) == fieldNumeric)
                        pieces.Add(fieldString);
                if (pieces.Count > 0)
                    return string.Join(", ", pieces);
            }
            return EnumMetadata<TEnum>.ValueToStringMap.TryGetValue(numericValue, out var stringValue) ? stringValue : enumValue.ToString();
        }

        public static bool HasZeroFlagStringValue<TEnum>() where TEnum : struct, Enum
        {
            return EnumMetadata<TEnum>.IsFlags && EnumMetadata<TEnum>.ValueToStringMap.ContainsKey(0UL);
        }

        public static string? GetZeroFlagStringValue<TEnum>() where TEnum : struct, Enum
        {
            if (!EnumMetadata<TEnum>.IsFlags)
                return null;
            return EnumMetadata<TEnum>.ValueToStringMap.TryGetValue(0UL, out var value) ? value : null;
        }

        public static bool IsValidFromString<TEnum>(string? value) where TEnum : struct, Enum
        {
            return TryParseInternal<TEnum>(value) is not null;
        }

        public static TEnum ParseFromString<TEnum>(string? value) where TEnum : struct, Enum
        {
            var result = TryParseInternal<TEnum>(value);
            if (result is not null)
                return result.Value;
            throw new FormatException($"'{value}' no es un valor válido para {typeof(TEnum).Name}");
        }

        public static bool IsValidListFromString<TEnum>(IEnumerable<string>? values) where TEnum : struct, Enum
        {
            if (values is null)
                return false;
            foreach (var value in values)
                if (TryParseInternal<TEnum>(value) is null)
                    return false;
            return true;
        }

        public static List<TEnum> ParseListFromString<TEnum>(IEnumerable<string> values) where TEnum : struct, Enum
        {
            ArgumentNullException.ThrowIfNull(values);
            var result = new List<TEnum>();
            foreach (var value in values)
                result.Add(ParseFromString<TEnum>(value));
            return result;
        }

        private static TEnum? TryParseInternal<TEnum>(string? value) where TEnum : struct, Enum
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;
            var trimmed = value.Trim();
            if (EnumMetadata<TEnum>.IsFlags && trimmed.Contains(','))
            {
                var tokens = trimmed.Split([','], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (tokens.Length == 0)
                    return null;
                ulong combined = 0UL;
                foreach (var token in tokens)
                {
                    var parsedToken = TryParseSingleToken<TEnum>(token);
                    if (parsedToken is null)
                        return null;
                    combined |= Convert.ToUInt64(parsedToken.Value);
                }
                return (TEnum)Enum.ToObject(typeof(TEnum), combined);
            }
            return TryParseSingleToken<TEnum>(trimmed);
        }

        private static TEnum? TryParseSingleToken<TEnum>(string token) where TEnum : struct, Enum
        {
            if (EnumMetadata<TEnum>.ParseMap.TryGetValue(token, out var parsedFromMap))
                return parsedFromMap;
            if (long.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out var signed))
            {
                var candidate = (TEnum)Enum.ToObject(typeof(TEnum), signed);
                if (EnumMetadata<TEnum>.IsFlags || Enum.IsDefined(typeof(TEnum), candidate))
                    return candidate;
            }
            if (ulong.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out var unsigned))
            {
                var candidate = (TEnum)Enum.ToObject(typeof(TEnum), unsigned);
                if (EnumMetadata<TEnum>.IsFlags || Enum.IsDefined(typeof(TEnum), candidate))
                    return candidate;
            }
            return null;
        }
    }
}