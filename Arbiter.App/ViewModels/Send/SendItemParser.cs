using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Arbiter.App.Models;

namespace Arbiter.App.ViewModels.Send;

internal static class SendItemParser
{
    private static readonly Regex WaitLineRegex =
        new(@"^@wait\s+(\d+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex DisconnectLineRegex =
        new("^@(disconnect|dc)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static bool TryParse(IReadOnlyList<string> lines, out List<SendEntry> items, out string? validationError)
    {
        validationError = null;
        items = [];

        for (var i = 0; i < lines.Count; i++)
        {
            var line = lines[i];
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var trimmed = line.Trim();

            // Allow comments
            if (trimmed.StartsWith('#') || trimmed.StartsWith("//"))
            {
                continue;
            }

            // Empty command
            if (trimmed == "@")
            {
                continue;
            }

            // Disconnect command
            var disconnectMatch = DisconnectLineRegex.Match(trimmed);
            if (disconnectMatch.Success)
            {
                items.Add(SendEntry.Disconnect);
                continue;
            }

            // Wait command
            var waitMatch = WaitLineRegex.Match(trimmed);
            if (waitMatch.Success)
            {
                if (!int.TryParse(waitMatch.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture,
                        out var ms))
                {
                    validationError = $"Invalid wait value on line {i + 1}";
                    items = [];
                    return false;
                }

                if (ms <= 0)
                {
                    validationError = $"Wait value must be a positive, non-zero integer on line {i + 1}";
                    items = [];
                    return false;
                }

                items.Add(new SendEntry(TimeSpan.FromMilliseconds(ms)));
                continue;
            }

            if (trimmed.StartsWith('@'))
            {
                validationError = $"Invalid command syntax on line {i + 1}";
                items = [];
                return false;
            }

            // Tokenize and parse a packet line with optional direction and mixed tokens
            try
            {
                var index = 0;
                var isServer = false;

                // Skip leading whitespace
                while (index < trimmed.Length && char.IsWhiteSpace(trimmed[index])) index++;

                // Direction
                if (index < trimmed.Length && (trimmed[index] == '<' || trimmed[index] == '>'))
                {
                    isServer = trimmed[index] == '<';
                    index++;
                }

                // Read command (must be hex 1-2 chars)
                SkipSpaces(trimmed, ref index);
                var commandToken = ReadToken(trimmed, ref index);
                if (string.IsNullOrEmpty(commandToken) || !IsHexByte(commandToken))
                {
                    validationError = $"Invalid packet format on line {i + 1}: missing or invalid command byte";
                    items = [];
                    return false;
                }

                var command = byte.Parse(commandToken, NumberStyles.HexNumber, CultureInfo.InvariantCulture);

                // Data bytes
                var dataBytes = new List<byte>(32);
                var patches = new List<EntityRef>();
                while (true)
                {
                    SkipSpaces(trimmed, ref index);
                    if (index >= trimmed.Length)
                    {
                        break;
                    }

                    // Support trailing comments starting with //
                    if (trimmed[index] == '/' && index + 1 < trimmed.Length && trimmed[index + 1] == '/')
                    {
                        break;
                    }

                    // Read next token or quoted string
                    if (IsQuote(trimmed[index]))
                    {
                        var quote = trimmed[index++];
                        var str = ReadQuoted(trimmed, ref index, quote);
                        if (str is null)
                        {
                            validationError = $"Unterminated string on line {i + 1}";
                            items = [];
                            return false;
                        }

                        var ascii = Encoding.ASCII.GetBytes(str);
                        dataBytes.AddRange(ascii);
                        continue;
                    }

                    var token = ReadToken(trimmed, ref index);
                    if (string.IsNullOrEmpty(token)) break;

                    if (IsHexByte(token))
                    {
                        dataBytes.Add(byte.Parse(token, NumberStyles.HexNumber, CultureInfo.InvariantCulture));
                        continue;
                    }

                    if (token[0] == '#' && token.Length > 1)
                    {
                        if (!TryParseNumberToken(token.AsSpan(1), out var bytes, out var err))
                        {
                            validationError = $"{err} on line {i + 1}";
                            items = [];
                            return false;
                        }

                        dataBytes.AddRange(bytes);
                        continue;
                    }

                    if (token[0] == '@' && token.Length > 1)
                    {
                        // Inline entity reference -> reserve 4 bytes and patch later
                        var name = token[1..];
                        var offset = dataBytes.Count;
                        dataBytes.AddRange(new byte[4]); // placeholder
                        patches.Add(new EntityRef(name, offset));
                        continue;
                    }

                    validationError = $"Invalid token '{token}' on line {i + 1}";
                    items = [];
                    return false;
                }

                items.Add(new SendEntry(isServer, command, dataBytes.ToArray(),
                    patches.Count > 0 ? patches.ToArray() : null));
            }
            catch
            {
                validationError = $"Invalid packet on line {i + 1}";
                items = [];
                return false;
            }
        }

        return true;

        static void SkipSpaces(string s, ref int idx)
        {
            while (idx < s.Length && char.IsWhiteSpace(s[idx])) idx++;
        }

        static bool IsQuote(char c) => c is '\'' or '"' or '`';

        static string? ReadQuoted(string s, ref int idx, char quote)
        {
            var sb = new StringBuilder();
            while (idx < s.Length)
            {
                var c = s[idx++];
                if (c == quote)
                {
                    return sb.ToString();
                }

                if (c == '\\' && idx < s.Length)
                {
                    var esc = s[idx++];
                    sb.Append(esc switch
                    {
                        'n' => '\n',
                        'r' => '\r',
                        't' => '\t',
                        _ => esc
                    });
                }
                else
                {
                    sb.Append(c);
                }
            }

            return null;
        }

        static string ReadToken(string s, ref int idx)
        {
            var start = idx;
            while (idx < s.Length && !char.IsWhiteSpace(s[idx]))
            {
                // stop at comment start
                if (s[idx] == '/' && idx + 1 < s.Length && s[idx + 1] == '/') break;
                if (IsQuote(s[idx])) break;
                idx++;
            }

            return s[start..idx];
        }

        static bool IsHexByte(string token)
            => token.Length is >= 1 and <= 2 && token
                .Select(ch => ch is >= '0' and <= '9' or >= 'a' and <= 'f' or >= 'A' and <= 'F')
                .All(isHex => isHex);

        static bool TryParseNumberToken(ReadOnlySpan<char> span, out byte[] bytes, out string? error)
        {
            error = null;
            bytes = [];
            if (!long.TryParse(span, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
            {
                error = $"Invalid number '#{span.ToString()}'";
                return false;
            }

            if (value >= 0)
            {
                switch (value)
                {
                    case <= byte.MaxValue:
                        bytes = [(byte)value];
                        return true;
                    case <= ushort.MaxValue:
                        bytes = [(byte)((value >> 8) & 0xFF), (byte)(value & 0xFF)]; // big-endian
                        return true;
                    case <= uint.MaxValue:
                        bytes =
                        [
                            (byte)((value >> 24) & 0xFF), (byte)((value >> 16) & 0xFF), (byte)((value >> 8) & 0xFF),
                            (byte)(value & 0xFF)
                        ];
                        return true;
                    default:
                        error = "Number exceeds 32-bit unsigned range";
                        break;
                }
            }
            else
            {
                switch (value)
                {
                    case >= sbyte.MinValue:
                        unchecked
                        {
                            bytes = [(byte)(value & 0xFF)];
                        }

                        return true;
                    case >= short.MinValue:
                        unchecked
                        {
                            bytes = [(byte)((value >> 8) & 0xFF), (byte)(value & 0xFF)];
                        }

                        return true;
                    case >= int.MinValue:
                        unchecked
                        {
                            bytes =
                            [
                                (byte)((value >> 24) & 0xFF), (byte)((value >> 16) & 0xFF), (byte)((value >> 8) & 0xFF),
                                (byte)(value & 0xFF)
                            ];
                        }

                        return true;
                    default:
                        error = "Negative number exceeds 32-bit range";
                        break;
                }
            }

            return false;
        }
    }
}
