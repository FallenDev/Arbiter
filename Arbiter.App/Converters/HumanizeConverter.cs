using System;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using Avalonia.Data.Converters;

namespace Arbiter.App.Converters;

public sealed class HumanizeConverter : IValueConverter
{
    public static HumanizeConverter Converter => new();
    
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null)
        {
            return null;
        }

        // If value is already a string, humanize it
        if (value is string s)
        {
            return HumanizeString(s);
        }

        // Try property named DisplayName first
        var type = value.GetType();
        var prop = type.GetProperty("DisplayName", BindingFlags.Public | BindingFlags.Instance);
        if (prop is null || !prop.CanRead)
        {
            return HumanizeString(value.ToString()!);
        }
        
        try
        {
            var text = prop.GetValue(value);
            return HumanizeString(text?.ToString()!);
        }
        catch
        {
            // ignore and fallback
        }

        // Fallback to ToString()
        return HumanizeString(value.ToString()!);
    }

    private static string HumanizeString(string input)
    {
        // Replace underscores with spaces
        input = input.Replace('_', ' ');
        
        // Add spaces between capital letters
        input = Regex.Replace(input, "([a-z])([A-Z])", "$1 $2");
        
        // Convert to title case
        input = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(input.ToLower());
        
        return input;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}