using System.Text.Json;
using System.Text.Json.Serialization;
using ESAM.GrowTracking.API.Serialization;

namespace ESAM.GrowTracking.API.Converters
{
    internal sealed class JsonEnumConverter<TEnum> : JsonConverter<TEnum> where TEnum : struct, Enum
    {
        private readonly bool _isNullable;

        public JsonEnumConverter(Type typeToConvert)
        {
            ArgumentNullException.ThrowIfNull(typeToConvert);
            _isNullable = Nullable.GetUnderlyingType(typeToConvert) != null;
        }

        public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                if (_isNullable)
                    return default;
                throw new JsonException($"No se puede convertir null al tipo {typeof(TEnum).Name}.");
            }
            if (reader.TokenType == JsonTokenType.Number)
            {
                if (reader.TryGetInt64(out var signed))
                {
                    var candidate = (TEnum)Enum.ToObject(typeof(TEnum), signed);
                    if (EnumMetadata<TEnum>.IsFlags || Enum.IsDefined(typeof(TEnum), candidate))
                        return candidate;
                    throw new JsonException($"El valor {signed} no está definido para el tipo de enumeración {typeof(TEnum).Name}.");
                }
                if (reader.TryGetUInt64(out var unsigned))
                {
                    var candidate = (TEnum)Enum.ToObject(typeof(TEnum), unsigned);
                    if (EnumMetadata<TEnum>.IsFlags || Enum.IsDefined(typeof(TEnum), candidate))
                        return candidate;
                    throw new JsonException($"El valor {unsigned} no está definido para el tipo de enumeración {typeof(TEnum).Name}.");
                }
                throw new JsonException($"No se puede parsear el número para el tipo de enumeración {typeof(TEnum).Name}.");
            }
            if (reader.TokenType == JsonTokenType.String)
            {
                var raw = reader.GetString() ?? string.Empty;
                try
                {
                    return EnumHelper.ParseFromString<TEnum>(raw);
                }
                catch (FormatException ex)
                {
                    throw new JsonException($"'{raw}' no es un valor válido para el tipo de enumeración {typeof(TEnum).Name}.", ex);
                }
            }
            throw new JsonException($"Token inesperado al parsear el tipo de enumeración. Token: {reader.TokenType}.");
        }

        public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
        {
            if (EnumMetadata<TEnum>.IsFlags && Convert.ToUInt64(value) == 0UL)
            {
                var zeroStr = EnumHelper.GetZeroFlagStringValue<TEnum>();
                if (zeroStr is not null)
                    writer.WriteStringValue(zeroStr);
                else
                    writer.WriteNumberValue(0);
                return;
            }
            writer.WriteStringValue(EnumHelper.GetStringValue(value));
        }
    }
}