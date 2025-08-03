using System;
using System.Globalization;
using Arbiter.Net.Client;
using Arbiter.Net.Server;
using Avalonia.Data.Converters;

namespace Arbiter.App.Converters;

public class PacketDirectionNameConverter : IValueConverter
{
    public static PacketDirectionNameConverter Converter => new();
    
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            ClientPacket => "Client",
            ServerPacket => "Server",
            _ => string.Empty,
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}