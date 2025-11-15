using System;
using System.Threading.Tasks;
using Arbiter.App.Extensions;
using Avalonia;
using Avalonia.Input.Platform;
using CommunityToolkit.Mvvm.Input;

namespace Arbiter.App.ViewModels.Send;

public partial class SendPacketViewModel
{
    private bool CanCopyToClipboard(string fieldName) => fieldName switch
    {
        "selection" => Math.Abs(SelectionEnd - SelectionStart) > 0,
        _ => true
    };

    [RelayCommand(CanExecute = nameof(CanCopyToClipboard))]
    private async Task CopyToClipboard(string fieldName)
    {
        var clipboard = Application.Current?.TryGetClipboard();
        if (clipboard is null)
        {
            return;
        }

        // Ensure that the selection is not reversed
        var selectionStart = Math.Min(SelectionStart, SelectionEnd);
        var selectionEnd = Math.Max(SelectionStart, SelectionEnd);

        var textToCopy = fieldName switch
        {
            "selection" => InputText.Substring(selectionStart, selectionEnd - selectionStart),
            _ => InputText
        };

        if (!string.IsNullOrEmpty(textToCopy))
        {
            await clipboard.SetTextAsync(textToCopy);
        }
    }

    [RelayCommand]
    private async Task PasteFromClipboard()
    {
        var clipboard = Application.Current?.TryGetClipboard();
        if (clipboard is null)
        {
            return;
        }

        var newText = await clipboard.TryGetTextAsync();
        if (!string.IsNullOrWhiteSpace(newText))
        {
            InputText = newText;
        }
    }

    [RelayCommand]
    private void ClearAll()
    {
        InputText = string.Empty;
    }
}