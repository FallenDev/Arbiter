using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Arbiter.App.Json;

public class HexNumberJsonConverter<T> : JsonConverter<T> where T:struct
{
    public override T Read(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var str = reader.GetString() ?? string.Empty;
            if (str.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                str = str[2..];
            }

            object output = type switch
            {
                _ when type == typeof(bool) => Convert.ToByte(str, 16) != 0,
                _ when type == typeof(byte) => Convert.ToByte(str, 16),
                _ when type == typeof(sbyte) => Convert.ToSByte(str, 16),
                _ when type == typeof(short) => Convert.ToInt16(str, 16),
                _ when type == typeof(ushort) => Convert.ToUInt16(str, 16),
                _ when type == typeof(int) => Convert.ToInt32(str, 16),
                _ when type == typeof(uint) => Convert.ToUInt32(str, 16),
                _ when type == typeof(long) => Convert.ToInt64(str, 16),
                _ when type == typeof(ulong) => Convert.ToUInt64(str, 16),
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