using System;
using System.Globalization;
using Arbiter.App.Models;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Arbiter.App.Converters;

public class EntityColorConverter : IValueConverter
{
    private static readonly SolidColorBrush DefaultBrush = new(Color.FromRgb(0xcd, 0xcf, 0xd2));
    private static readonly SolidColorBrush PlayerBrush = new(Color.FromRgb(0x82, 0xbf, 0xf7));
    private static readonly SolidColorBrush MundaneBrush = new(Color.FromRgb(0x83, 0xe6, 0x7a));
    private static readonly SolidColorBrush ItemBrush = new(Color.FromRgb(0xfa, 0xdf, 0x79));
    private static readonly SolidColorBrush MonsterBrush = new(Color.FromRgb(0xee, 0x80, 0x71));
    private static readonly SolidColorBrush ReactorBrush = new(Color.FromRgb(0xcd, 0xcf, 0xd2));
    
    public static EntityColorConverter Converter => new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not EntityFlags flags)
        {
            return DefaultBrush;
        }

        if (flags.HasFlag(EntityFlags.Player))
        {
            return PlayerBrush;
        }

        if (flags.HasFlag(EntityFlags.Mundane))
        {
            return MundaneBrush;
        }

        if (flags.HasFlag(EntityFlags.Monster))
        {
            return MonsterBrush;
        }

        if (flags.HasFlag(EntityFlags.Item))
        {
            return ItemBrush;
        }

        return ReactorBrush;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}