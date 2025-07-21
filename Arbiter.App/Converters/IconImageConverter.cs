using System;
using System.Globalization;
using System.IO;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;

namespace Arbiter.App.Converters;

public class IconImageConverter : IValueConverter
{
    public static IconImageConverter Converter => new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not WindowIcon icon)
        {
            return null;
        }

        using var stream = new MemoryStream();
        icon.Save(stream);
        stream.Position = 0;
        var result = new Bitmap(stream);

        return result;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}