using System;
using System.Globalization;
using System.Reflection;
using Avalonia.Data.Converters;

namespace Arbiter.App.Converters;

public sealed class DisplayNameConverter : IValueConverter
{
    public static DisplayNameConverter Converter => new();
    
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null)
        {
            return null;
        }

        // If value is already a string, use it
        if (value is string s)
            return s;

        // Try property named DisplayName first
        var type = value.GetType();
        var prop = type.GetProperty("DisplayName", BindingFlags.Public | BindingFlags.Instance);
        if (prop is null || !prop.CanRead)
        {
            return value.ToString();
        }
        
        try
        {
            var text = prop.GetValue(value);
            return text?.ToString();
        }
        catch
        {
            // ignore and fallback
        }

        // Fallback to ToString()
        return value.ToString();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
