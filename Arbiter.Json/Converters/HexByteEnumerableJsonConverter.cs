using System.Text.Json;
using System.Text.Json.Serialization;

namespace Arbiter.Json.Converters;

public class HexByteEnumerableJsonConverter : JsonConverter<IEnumerable<byte>>
{
    public override IEnumerable<byte> Read(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException("Expected string value for hex byte array");
        }

        var hexString = reader.GetString();
        if (string.IsNullOrWhiteSpace(hexString))
        {
            return [];
        }

        return hexString.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(hex => byte.Parse(hex, System.Globalization.NumberStyles.HexNumber));
    }

    public override void Write(Utf8JsonWriter writer, IEnumerable<byte> value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(string.Join(" ", value.Select(b => b.ToString("X2"))));
    }
}