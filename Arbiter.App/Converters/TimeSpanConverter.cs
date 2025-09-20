using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Arbiter.App.Converters;

public class TimeSpanConverter : IValueConverter
{
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