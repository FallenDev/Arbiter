using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Microsoft.Extensions.Logging;

namespace Arbiter.App.Converters;

public class LogLevelBrushConverter : IValueConverter
{
    private static readonly SolidColorBrush DefaultBrush = new(Color.FromRgb(0xcd, 0xcf, 0xd2));
    private static readonly SolidColorBrush DebugBrush = new(Color.FromRgb(0x82, 0xbf, 0xf7));
    private static readonly SolidColorBrush InfoBrush = new(Color.FromRgb(0xcd, 0xcf, 0xd2));
    private static readonly SolidColorBrush WarningBrush = new(Color.FromRgb(0xfa, 0xdf, 0x79));
    private static readonly SolidColorBrush ErrorBrush = new(Color.FromRgb(0xee, 0x80, 0x71));
    
    public static LogLevelBrushConverter Converter => new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not LogLevel logLevel)
        {
            return DefaultBrush;
        }

        return logLevel switch
        {
            LogLevel.Trace => DebugBrush,
            LogLevel.Debug => DebugBrush,
            LogLevel.Information => InfoBrush,
            LogLevel.Warning => WarningBrush,
            LogLevel.Error => ErrorBrush,
            LogLevel.Critical => ErrorBrush,
            _ => DefaultBrush,
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}