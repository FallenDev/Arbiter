using System;
using System.Globalization;
using Arbiter.Net.Filters;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Arbiter.App.Converters;

public class PacketFilterColorConverter : IValueConverter
{
    private static readonly SolidColorBrush DefaultBrush = new(Color.FromRgb(0xcd, 0xcf, 0xd2));
    private static readonly SolidColorBrush BlockedBrush = new(Color.FromRgb(0x71, 0x74, 0x79));

    public static PacketFilterColorConverter Converter => new();
    
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not NetworkFilterAction action)
        {
            return DefaultBrush;
        }

        return action switch
        {
            NetworkFilterAction.Block => BlockedBrush,
            _ => DefaultBrush
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}