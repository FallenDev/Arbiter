using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Arbiter.App.Models;
using Arbiter.App.Threading;
using Arbiter.Net;
using Arbiter.Net.Client;
using Arbiter.Net.Server;
using Avalonia.Threading;
using Microsoft.Extensions.Logging;

namespace Arbiter.App.ViewModels.Send;

public partial class SendPacketViewModel
{
    private static readonly char[] NewLineCharacters = ['\r', '\n'];

    private static readonly Regex PacketLineRegex =
        new(@"^(<|>)?\s*([0-9a-f]{1,2})(?:\s+([0-9a-f]{1,2}))*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex WaitLineRegex =
        new(@"^@wait\s+(\d+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex DisconnectLineRegex =
        new(@"^@(disconnect|dc)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    
    private readonly List<SendEntry> _parsedItems = [];
    private readonly Debouncer _validationDebouncer = new(TimeSpan.FromMilliseconds(300), Dispatcher.UIThread);
    
    private void PerformValidation()
    {
        var lines = InputText.Split(NewLineCharacters, StringSplitOptions.RemoveEmptyEntries);
        var success = TryParseSendItems(lines, out var items, out var validationError);
        ValidationError = success ? null : validationError;
        ValidationErrorOpacity = !string.IsNullOrWhiteSpace(ValidationError) ? 1 : 0;
        
        HasPackets = success && items.Any(x => x.Packet is not null);
        HasDisconnects = success && items.Any(x => x.IsDisconnect);
        
        StartSendCommand.NotifyCanExecuteChanged();

        if (validationError is not null)
        {
            throw new ValidationException(validationError);
        }
    }
    
    // Pure parsing routine: parse lines to send items without mutating state
    private bool TryParseSendItems(IReadOnlyList<string> lines, out List<SendEntry> items, out string? validationError)
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
                if (!int.TryParse(waitMatch.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var ms))
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

            var match = PacketLineRegex.Match(trimmed);
            if (!match.Success)
            {
                validationError = $"Invalid packet format on line {i + 1}";
                items = [];
                return false;
            }

            try
            {
                var caret = match.Groups[1].Value;
                var command = byte.Parse(match.Groups[2].Value, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                var data = match.Groups[3].Captures.Select(x => byte.Parse(x.Value, NumberStyles.HexNumber, CultureInfo.InvariantCulture));

                NetworkPacket packet = caret switch
                {
                    "<" => new ServerPacket(command, data),
                    _ => new ClientPacket(command, data)
                };

                items.Add(new SendEntry(packet));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing packet on line {Line}", i + 1);
                validationError = $"Invalid packet on line {i + 1}";
                items = [];
                return false;
            }
        }

        return true;
    }
}