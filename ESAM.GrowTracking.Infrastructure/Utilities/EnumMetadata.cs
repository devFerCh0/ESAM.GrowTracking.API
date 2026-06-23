using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;

namespace ESAM.GrowTracking.Infrastructure.Utilities
{
    public static class EnumMetadata<TEnum> where TEnum : struct, Enum
    {
        public static readonly bool IsFlags = typeof(TEnum).GetCustomAttribute<FlagsAttribute>() != null;
        public static readonly IReadOnlyList<(ulong NumericValue, string StringValue, TEnum EnumValue)> Fields;
        public static readonly IReadOnlyDictionary<string, TEnum> ParseMap;
        public static readonly IReadOnlyDictionary<ulong, string> ValueToStringMap;

        static EnumMetadata()
        {
            var type = typeof(TEnum);
            var rawFields = type.GetFields(BindingFlags.Public | BindingFlags.Static);
            var fields = new List<(ulong, string, TEnum)>(rawFields.Length);
            var parseMap = new Dictionary<string, TEnum>(StringComparer.OrdinalIgnoreCase);
            var valueToStringMap = new Dictionary<ulong, string>();
            foreach (var field in rawFields)
            {
                var enumValue = (TEnum)field.GetValue(null)!;
                var numericValue = Convert.ToUInt64(enumValue);
                var display = field.GetCustomAttribute<DisplayAttribute>();
                var desc = field.GetCustomAttribute<DescriptionAttribute>();
                var enumMember = field.GetCustomAttribute<EnumMemberAttribute>();
                var stringValue = ResolveStringValue(display, desc, enumMember, enumValue);
                fields.Add((numericValue, stringValue, enumValue));
                if (!valueToStringMap.ContainsKey(numericValue))
                    valueToStringMap[numericValue] = stringValue;
                RegisterEntry(parseMap, field.Name, enumValue);
                RegisterEntry(parseMap, enumValue.ToString(), enumValue);
                RegisterEntry(parseMap, numericValue.ToString(CultureInfo.InvariantCulture), enumValue);
                RegisterEntry(parseMap, stringValue, enumValue);
                if (display?.Name is { Length: > 0 } displayName)
                    RegisterEntry(parseMap, displayName, enumValue);
                if (desc?.Description is { Length: > 0 } description)
                    RegisterEntry(parseMap, description, enumValue);
                if (enumMember?.Value is { Length: > 0 } memberValue)
                    RegisterEntry(parseMap, memberValue, enumValue);
            }
            Fields = fields.AsReadOnly();
            ParseMap = parseMap;
            ValueToStringMap = valueToStringMap;
        }

        private static string ResolveStringValue(DisplayAttribute? display, DescriptionAttribute? desc, EnumMemberAttribute? enumMember, TEnum enumValue)
        {
            if (display?.Name is { Length: > 0 } displayName)
                return displayName;
            if (desc?.Description is { Length: > 0 } description)
                return description;
            if (enumMember?.Value is { Length: > 0 } memberValue)
                return memberValue;
            return enumValue.ToString();
        }

        private static void RegisterEntry(Dictionary<string, TEnum> map, string? key, TEnum value)
        {
            if (!string.IsNullOrWhiteSpace(key) && !map.ContainsKey(key))
                map[key] = value;
        }
    }
}