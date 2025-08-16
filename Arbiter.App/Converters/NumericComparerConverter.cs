using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Arbiter.App.Converters;

public class NumericComparerConverter : IValueConverter
{
    public static NumericComparerConverter IsZero => new((long value, long _) => value == 0);
    public static NumericComparerConverter NotZero => new((long value, long _) => value != 0);
    public static NumericComparerConverter IsPositive => new((long value, long _) => value > 0);
    public static NumericComparerConverter IsNegative => new((long value, long _) => value < 0);
    public static NumericComparerConverter IsEven => new((long value, long _) => value % 2 == 0);
    public static NumericComparerConverter IsOdd => new((long value, long _) => value % 2 != 0);
    public static NumericComparerConverter IsGreaterThan => new((long lhs, long rhs) => lhs > rhs);
    public static NumericComparerConverter IsLessThan => new((long lhs, long rhs) => lhs < rhs);
    public static NumericComparerConverter IsGreaterOrEqual => new((long lhs, long rhs) => lhs >= rhs);
    public static NumericComparerConverter IsLessOrEqual => new((long lhs, long rhs) => lhs <= rhs);
    public static NumericComparerConverter IsEqual => new((long lhs, long rhs) => lhs == rhs);
    public static NumericComparerConverter IsNotEqual => new((long lhs, long rhs) => lhs != rhs);
    public static NumericComparerConverter IsEvenlyDivisibleBy => new((long lhs, long rhs) => lhs % rhs == 0);
    
    private readonly Func<long, long, bool> _comparer;

    public NumericComparerConverter()
        : this((long _, long _) => true)
    {
    }

    public NumericComparerConverter(Func<int, int, bool> comparer)
        : this((long lhs, long rhs) => comparer((int)lhs, (int)rhs))
    {
    }

    public NumericComparerConverter(Func<long, long, bool> comparer)
    {
        _comparer = comparer;
    }

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        long rhs = 0;
        if (parameter is not null)
        {
            try
            {
                rhs = ConvertToLong(parameter, culture);
            }
            catch
            {
                throw new FormatException("Invalid parameter for NumericComparerConverter.");
            }
        }

        if (value == null)
        {
            return false;
        }

        try
        {
            var lhs = ConvertToLong(value, culture);
            return _comparer(lhs, rhs);
        }
        catch
        {
            return false;
        }
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }

    private static long ConvertToLong(object value, CultureInfo culture)
    {
        return value switch
        {
            sbyte i8 => i8,
            byte u8 => u8,
            short i16 => i16,
            ushort u16 => u16,
            int i32 => i32,
            uint u32 => u32,
            long i64 => i64,
            ulong u64 => unchecked((long)u64),
            bool boolean => boolean ? 1L : 0L,
            char singleChar => singleChar,
            decimal dec => (long)dec,
            double dp => (long)dp,
            float fp => (long)fp,
            string str => ParseStringToLong(str, culture),
            IConvertible conv => conv.ToInt64(culture),
            _ => throw new InvalidCastException($"Unsupported type {value.GetType()} for NumericComparerConverter.")
        };
    }

    private static long ParseStringToLong(string value, CultureInfo? culture = null)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return 0;
        }

        var stringSpan = value.AsSpan().Trim();

        if (stringSpan.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            var hexString = stringSpan[2..];
            if (ulong.TryParse(hexString, NumberStyles.HexNumber, culture, out var ulongValue))
                return unchecked((long)ulongValue);
            if (long.TryParse(hexString, NumberStyles.HexNumber, culture, out var longValue))
                return longValue;
            throw new FormatException($"Can't parse hex '{stringSpan}'");
        }

        if (stringSpan.StartsWith("0b", StringComparison.OrdinalIgnoreCase))
        {
            var binaryString = stringSpan[2..];
            ulong acc = 0;
            foreach (var bit in binaryString)
            {
                if (bit == '0')
                {
                    acc <<= 1;
                }
                else if (bit == '1')
                {
                    acc = (acc << 1) | 1;
                }
                else throw new FormatException($"Invalid binary char '{bit}'");
            }

            return unchecked((long)acc);
        }

        if (long.TryParse(stringSpan, NumberStyles.Integer, culture, out var intLongValue))
        {
            return intLongValue;
        }

        if (ulong.TryParse(stringSpan, NumberStyles.Integer, culture, out var intUlongValue))
        {
            return unchecked((long)intUlongValue);
        }

        throw new FormatException($"Can't parse '{stringSpan}' to a number.");
    }
}