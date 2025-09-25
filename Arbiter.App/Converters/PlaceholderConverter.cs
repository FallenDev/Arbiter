using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace Arbiter.App.Converters;

public sealed class PlaceholderConverter : IValueConverter
{
    public static PlaceholderConverter Converter => new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var s = value as string;
        if (string.IsNullOrEmpty(s))
        {
            // parameter is the placeholder text, e.g., "<unknown>"
            return parameter ?? string.Empty;
        }
        return s;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // Not supported; one-way usage only
        return new BindingNotification(new NotSupportedException(), BindingErrorType.Error);
    }
}
