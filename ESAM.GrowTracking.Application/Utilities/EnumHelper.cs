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

        public static bool TryGetZeroFlagStringValue<TEnum>(out string? value) where TEnum : struct, Enum
        {
            value = null;
            if (!EnumMetadata<TEnum>.IsFlags)
                return false;
            return EnumMetadata<TEnum>.ValueToStringMap.TryGetValue(0UL, out value);
        }

        public static bool TryParseFromString<TEnum>(string? value, out TEnum result) where TEnum : struct, Enum
        {
            result = default;
            if (string.IsNullOrWhiteSpace(value))
                return false;
            var trimmed = value.Trim();
            if (EnumMetadata<TEnum>.IsFlags && trimmed.Contains(','))
            {
                var tokens = trimmed.Split([','], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                ulong combined = 0UL;
                foreach (var token in tokens)
                {
                    if (!TryParseSingleToken<TEnum>(token, out var parsed))
                        return false;
                    combined |= Convert.ToUInt64(parsed);
                }
                result = (TEnum)Enum.ToObject(typeof(TEnum), combined);
                return true;
            }
            return TryParseSingleToken(trimmed, out result);
        }

        private static bool TryParseSingleToken<TEnum>(string token, out TEnum result) where TEnum : struct, Enum
        {
            if (EnumMetadata<TEnum>.ParseMap.TryGetValue(token, out result))
                return true;
            if (long.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out var signed))
            {
                var candidate = (TEnum)Enum.ToObject(typeof(TEnum), signed);
                if (EnumMetadata<TEnum>.IsFlags || Enum.IsDefined(typeof(TEnum), candidate))
                {
                    result = candidate;
                    return true;
                }
            }
            else if (ulong.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out var unsigned))
            {
                var candidate = (TEnum)Enum.ToObject(typeof(TEnum), unsigned);
                if (EnumMetadata<TEnum>.IsFlags || Enum.IsDefined(typeof(TEnum), candidate))
                {
                    result = candidate;
                    return true;
                }
            }
            result = default;
            return false;
        }

        public static TEnum ParseFromString<TEnum>(string value) where TEnum : struct, Enum
        {
            if (TryParseFromString<TEnum>(value, out var result))
                return result;
            throw new FormatException($"'{value}' no es un valor válido para {typeof(TEnum).Name}");
        }

        public static bool TryParseListFromString<TEnum>(IEnumerable<string>? values, out List<TEnum> result) where TEnum : struct, Enum
        {
            result = [];
            if (values is null)
                return false;
            var parsed = new List<TEnum>();
            foreach (var value in values)
            {
                if (!TryParseFromString<TEnum>(value, out var item))
                    return false;
                parsed.Add(item);
            }
            result = parsed;
            return true;
        }
    }
}