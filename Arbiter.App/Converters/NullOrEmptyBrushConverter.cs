using System;
using System.Globalization;
using Avalonia;
using Avalonia.Media;
using Avalonia.Data.Converters;

namespace Arbiter.App.Converters;

public sealed class NullOrEmptyBrushConverter : IValueConverter
{
    public static NullOrEmptyBrushConverter Converter => new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var s = value as string;
        if (!string.IsNullOrEmpty(s))
        {
            return AvaloniaProperty.UnsetValue;
        }
        
        // Expect parameter to be an IBrush; if missing, fall back to a dim gray
        if (parameter is IBrush brush)
        {
            return brush;
        }
        return new SolidColorBrush(Color.FromRgb(136, 136, 136)); // #888
        // Leave binding target unset to keep default brush
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
