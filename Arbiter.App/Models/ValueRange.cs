using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Arbiter.App.Models;

public struct ValueRange<T>(T min, T max)
    where T : struct, IComparable<T>, IComparable, IEquatable<T>
{
    public T Min { get; set; } = min;
    public T Max { get; set; } = max;

    public ValueRange(T value) : this(value, value)
    {
    }

    public bool Contains(T value) => value.CompareTo(Min) >= 0 && value.CompareTo(Max) <= 0;

    public static bool TryParseByteRange(string value, out ValueRange<byte> result)
        => TryParseByteRange(value, NumberStyles.Integer, out result);

    public override string ToString()
    {
        return (EqualityComparer<T>.Default.Equals(Min, Max) ? Min.ToString() : $"{Min} to {Max}") ?? string.Empty;
    }

    public static bool TryParseByteRange(string value, NumberStyles style, out ValueRange<byte> result)
    {
        result = default;

        var tokens = value.Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var minValueText = tokens.First();
        var maxValueText = tokens.Last();

        // Despite what the documentation says, AllowHexSpecifier does not parse this properly
        if (style.HasFlag(NumberStyles.AllowHexSpecifier) && minValueText.StartsWith("0x"))
        {
            minValueText = minValueText[2..];
        }

        if (style.HasFlag(NumberStyles.AllowHexSpecifier) && maxValueText.StartsWith("0x"))
        {
            maxValueText = maxValueText[2..];       
        }

        if (!byte.TryParse(minValueText, style  ,null, out var minValue))
        {
            return false;
        }

        if (!byte.TryParse(maxValueText, style, null, out var maxValue))
        {
            return false;
        }

        result = new ValueRange<byte>(minValue, maxValue);
        return true;
    }

    public static ValueRange<byte> ParseByteRange(string value, NumberStyles style = NumberStyles.Integer)
    {
        if (!TryParseByteRange(value, style, out var result))
            throw new FormatException("Invalid byte value range");

        return result;
    }
}