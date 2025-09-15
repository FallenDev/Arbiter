using System.Text;

namespace Arbiter.App.Extensions;

public static class StringExtensions
{
    public static string ToNaturalWording(this string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        var result = new StringBuilder(value.Length + 4);
        result.Append(value[0]);

        for (var i = 1; i < value.Length; i++)
        {
            var current = value[i];
            var prev = value[i - 1];
            var next = i + 1 < value.Length ? value[i + 1] : '\0';

            // Insert space at natural boundaries:
            // 1) Uppercase following a lowercase (e.g., "myValue" -> "my Value").
            // 2) Transition like "ABCd" -> space before 'd' (upper followed by lower after an upper sequence).
            // 3) Letter <-> Digit transitions (e.g., "Value2" -> "Value 2", "V2Name" -> "V 2 Name").
            bool boundary = false;

            if (char.IsUpper(current))
            {
                if (char.IsLower(prev) || (next != '\0' && char.IsLower(next)))
                {
                    boundary = true;
                }
            }
            else if ((char.IsLetter(current) && char.IsDigit(prev)) || (char.IsDigit(current) && char.IsLetter(prev)))
            {
                boundary = true;
            }

            if (boundary)
            {
                result.Append(' ');
            }

            result.Append(current);
        }

        return result.ToString();
    }
}