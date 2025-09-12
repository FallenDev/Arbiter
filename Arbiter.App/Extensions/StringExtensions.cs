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
            if (char.IsUpper(value[i]))
            {
                if (char.IsLower(value[i - 1]) ||
                    (i + 1 < value.Length && char.IsLower(value[i + 1])))
                {
                    result.Append(' ');
                }
            }

            result.Append(value[i]);
        }

        return result.ToString();
    }
}