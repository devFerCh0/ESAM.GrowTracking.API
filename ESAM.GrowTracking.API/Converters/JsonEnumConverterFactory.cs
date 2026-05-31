using System.Text.Json;
using System.Text.Json.Serialization;

namespace ESAM.GrowTracking.API.Converters
{
    public sealed class JsonEnumConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
        {
            var underlyingType = Nullable.GetUnderlyingType(typeToConvert) ?? typeToConvert;
            return underlyingType.IsEnum;
        }

        public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            var enumType = Nullable.GetUnderlyingType(typeToConvert) ?? typeToConvert;
            var converterType = typeof(JsonEnumConverter<>).MakeGenericType(enumType);
            return (JsonConverter?)Activator.CreateInstance(converterType, [typeToConvert]);
        }
    }
}