using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Arbiter.Json.Converters;

public class HexNumberJsonConverter<T> : JsonConverter<T> where T : struct
{
    public override T Read(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var str = (reader.GetString() ?? string.Empty).AsSpan();
            if (str.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                str = str[2..];
            }

            object output = type switch
            {
                _ when type == typeof(bool) => byte.Parse(str, NumberStyles.HexNumber) != 0,
                _ when type == typeof(byte) => byte.Parse(str, NumberStyles.HexNumber),
                _ when type == typeof(sbyte) => sbyte.Parse(str, NumberStyles.HexNumber),
                _ when type == typeof(short) => short.Parse(str, NumberStyles.HexNumber),
                _ when type == typeof(ushort) => ushort.Parse(str, NumberStyles.HexNumber),
                _ when type == typeof(int) => int.Parse(str, NumberStyles.HexNumber),
                _ when type == typeof(uint) => uint.Parse(str, NumberStyles.HexNumber),
                _ when type == typeof(long) => long.Parse(str, NumberStyles.HexNumber),
                _ when type == typeof(ulong) => ulong.Parse(str, NumberStyles.HexNumber),
                _ => throw new JsonException("Unsupported type")
            };

            return (T)output;
        }

        if (reader.TokenType != JsonTokenType.Number)
        {
            throw new JsonException("Unsupported token type");
        }

        object numberOutput = type switch
        {
            _ when type == typeof(bool) => reader.GetByte() != 0,
            _ when type == typeof(byte) => reader.GetByte(),
            _ when type == typeof(sbyte) => reader.GetSByte(),
            _ when type == typeof(short) => reader.GetInt16(),
            _ when type == typeof(ushort) => reader.GetUInt16(),
            _ when type == typeof(int) => reader.GetInt32(),
            _ when type == typeof(uint) => reader.GetUInt32(),
            _ when type == typeof(long) => reader.GetInt64(),
            _ when type == typeof(ulong) => reader.GetUInt64(),
            _ => throw new JsonException("Unsupported type")
        };

        return (T)numberOutput;
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        if (value is not IConvertible c)
        {
            throw new JsonException("Value is not convertible");
        }

        var type = typeof(T);
        var hexWidth = type switch
        {
            _ when type == typeof(bool) => 2,
            _ when type == typeof(byte) => 2,
            _ when type == typeof(sbyte) => 2,
            _ when type == typeof(short) => 4,
            _ when type == typeof(ushort) => 4,
            _ when type == typeof(int) => 8,
            _ when type == typeof(uint) => 8,
            _ when type == typeof(long) => 16,
            _ when type == typeof(ulong) => 16,
            _ => throw new JsonException("Unsupported type")
        };

        var longValue = c.ToUInt64(CultureInfo.InvariantCulture);
        var hexString = longValue.ToString("X" + hexWidth, CultureInfo.InvariantCulture);
        writer.WriteStringValue(hexString);
    }
}