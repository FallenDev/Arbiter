using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Arbiter.App.Models;
using Arbiter.App.Threading;
using Avalonia.Threading;

namespace Arbiter.App.ViewModels.Send;

public partial class SendPacketViewModel
{
    private static readonly char[] NewLineCharacters = ['\r', '\n'];
    
    private readonly List<SendEntry> _parsedItems = [];
    private readonly Debouncer _validationDebouncer = new(TimeSpan.FromMilliseconds(300), Dispatcher.UIThread);
    
    private void PerformValidation()
    {
        var lines = InputText.Split(NewLineCharacters, StringSplitOptions.RemoveEmptyEntries);
        var success = TryParseSendItems(lines, out var items, out var validationError);
        ValidationError = success ? null : validationError;
        ValidationErrorOpacity = !string.IsNullOrWhiteSpace(ValidationError) ? 1 : 0;
        
        HasPackets = success && items.Any(x => x.Packet is not null || x.Command.HasValue);
        HasDisconnects = success && items.Any(x => x.IsDisconnect);
        
        StartSendCommand.NotifyCanExecuteChanged();

        if (validationError is not null)
        {
            throw new ValidationException(validationError);
        }
    }
    
    // Pure parsing routine: parse lines to send items without mutating state
    private static bool TryParseSendItems(IReadOnlyList<string> lines, out List<SendEntry> items, out string? validationError)
    {
        return SendItemParser.TryParse(lines, out items, out validationError);
    }
}