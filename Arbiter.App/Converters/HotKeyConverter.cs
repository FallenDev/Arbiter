using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;

namespace Arbiter.App.Converters;

public enum HotKeyConverterMode
{
    GestureText,
    HasHotKey,
}

public class HotKeyConverter : IValueConverter
{
    public static HotKeyConverter GestureText => new();
    public static HotKeyConverter HasHotKey => new(HotKeyConverterMode.HasHotKey);

    public HotKeyConverterMode Mode { get; }

    public HotKeyConverter() : this(HotKeyConverterMode.GestureText)
    {
    }

    public HotKeyConverter(HotKeyConverterMode mode)
    {
        Mode = mode;
    }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return Mode switch
        {
            HotKeyConverterMode.GestureText => ConvertGestureText(value),
            HotKeyConverterMode.HasHotKey => ConvertHasHotKey(value),
            _ => throw new NotImplementedException(),
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }

    private static string? ConvertGestureText(object? value)
    {
        if (value is not AvaloniaObject obj)
        {
            return null;
        }

        var keyGesture = HotKeyManager.GetHotKey(obj);
        var gestureText = keyGesture?.ToString().Replace(" ", string.Empty, StringComparison.Ordinal);
        
        gestureText = gestureText?.Replace("Oem4", "[");
        gestureText = gestureText?.Replace("OemCloseBrackets", "]");
        
        return gestureText;
    }

    private static bool ConvertHasHotKey(object? value)
    {
        if (value is not AvaloniaObject obj)
        {
            return false;
        }
        
        var keyGesture = HotKeyManager.GetHotKey(obj);
        return keyGesture is not null;
    }
}