using System;
using System.Globalization;
using Arbiter.App.Models;
using Arbiter.Net;
using Arbiter.Net.Client;
using Arbiter.Net.Server;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Arbiter.App.Converters;

public class PacketDirectionColorConverter : IValueConverter
{
    private static readonly SolidColorBrush DefaultBrush = new(Color.FromRgb(0xcd, 0xcf, 0xd2));
    private static readonly SolidColorBrush ClientBrush = new(Color.FromRgb(0x82, 0xbf, 0xf7));
    private static readonly SolidColorBrush ServerBrush = new(Color.FromRgb(0x83, 0xe6, 0x7a));
    
    public static PacketDirectionColorConverter Converter => new();
    
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            PacketDirection => value switch
            {
                PacketDirection.Client => ClientBrush,
                PacketDirection.Server => ServerBrush,
                _ => DefaultBrush
            },
            NetworkPacket => value switch
            {
                ClientPacket => ClientBrush,
                ServerPacket => ServerBrush,
                _ => DefaultBrush
            },
            _ => null
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}