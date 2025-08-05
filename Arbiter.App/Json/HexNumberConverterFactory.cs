using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Arbiter.App.Json;

public class HexNumberConverterFactory : JsonConverterFactory
{
    private static readonly Type[] SupportedTypes =
    [
        typeof(bool),
        typeof(byte), typeof(sbyte),
        typeof(short), typeof(ushort),
        typeof(int), typeof(uint),
        typeof(long), typeof(ulong)
    ];

    public override bool CanConvert(Type typeToConvert) => SupportedTypes.Contains(typeToConvert);

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var converterType = typeof(HexNumberJsonConverter<>).MakeGenericType(typeToConvert);
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }
}