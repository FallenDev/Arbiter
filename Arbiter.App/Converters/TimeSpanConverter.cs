using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Arbiter.App.Converters;

public class TimeSpanConverter : IValueConverter
{
    private readonly Func<TimeSpan, string> _formatter;

    public static TimeSpanConverter Milliseconds => new(ts =>
        ts.TotalMilliseconds < 1000
            ? ts.TotalMilliseconds.ToString("F0") + " ms"
            : ts.TotalSeconds.ToString("F0") + "s");
    
    public static TimeSpanConverter Seconds => new(ts => ts.TotalSeconds.ToString("F0") + "s");

    public TimeSpanConverter(Func<TimeSpan, string>? formatter = null)
    {
        _formatter = formatter ?? Format;
    }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not TimeSpan timeSpan)
        {
            return null;
        }

        if (timeSpan == TimeSpan.Zero)
        {
            return parameter as string ?? "Zero";
        }

        return _formatter.Invoke(timeSpan);
    }

    private static string Format(TimeSpan timeSpan)
    {
        var hours = (int)timeSpan.TotalHours;
        var minutes = timeSpan.Minutes;
        var seconds = timeSpan.Seconds;
        var milliseconds = timeSpan.Milliseconds;

        if (hours > 0)
        {
            return $"{hours} {(hours == 1 ? "hour" : "hours")}";
        }

        if (minutes > 0)
        {
            return $"{minutes} {(minutes == 1 ? "minute" : "minutes")}";
        }

        if (seconds > 0 || milliseconds == 0)
        {
            return $"{seconds} {(seconds == 1 ? "second" : "seconds")}";
        }

        return $"{milliseconds / 1000.0:F1} seconds";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}