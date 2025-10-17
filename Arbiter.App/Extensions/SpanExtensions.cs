using System;
using System.Text;

namespace Arbiter.App.Extensions;

public static class SpanExtensions
{
    public static string ToHexString(this byte[] buffer, string delimiter = "") =>
        ToHexString(buffer.AsSpan(), delimiter);

    public static string ToHexString(this Memory<byte> memory, string delimiter = "") =>
        ToHexString(memory.Span, delimiter);

    public static string ToHexString(this ReadOnlyMemory<byte> memory, string delimiter = "") =>
        ToHexString(memory.Span, delimiter);

    public static string ToHexString(this Span<byte> span, string delimiter = "") =>
        ToHexString((ReadOnlySpan<byte>)span, delimiter);

    public static string ToHexString(this ReadOnlySpan<byte> span, string delimiter = "")
    {
        var totalLength = span.Length * 2 + (span.Length - 1) * delimiter.Length;
        var hasDelimiter = !string.IsNullOrEmpty(delimiter);

        var sb = new StringBuilder(totalLength);
        for (var i = 0; i < span.Length; i++)
        {
            sb.Append(span[i].ToString("X2"));

            if (hasDelimiter && i < span.Length - 1)
            {
                sb.Append(' ');
            }
        }

        return sb.ToString();
    }
}