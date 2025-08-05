using System.Text.Json;
using System.Text.Json.Serialization;

namespace Arbiter.Json.Converters;

public class HexByteEnumerableConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert == typeof(byte[]) || typeof(IEnumerable<byte>).IsAssignableFrom(typeToConvert);
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var converterType = typeof(HexByteEnumerableJsonConverter);
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }
}