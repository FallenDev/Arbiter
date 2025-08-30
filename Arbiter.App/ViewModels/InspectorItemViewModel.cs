using System.Threading.Tasks;
using Arbiter.App.Extensions;
using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Arbiter.App.ViewModels;

public abstract partial class InspectorItemViewModel : ViewModelBase
{
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private int _order = int.MaxValue;

    [ObservableProperty] private string? _toolTip;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(CanReveal))]
    private char? _maskCharacter;

    [ObservableProperty] private bool _isRevealed = true;

    public bool CanReveal => MaskCharacter is not null;

    [RelayCommand]
    private async Task RequestCopy()
    {
        var textToCopy = GetCopyableValue();

        var clipboard = Application.Current?.TryGetClipboard();
        if (clipboard is not null && textToCopy is not null)
        {
            await clipboard.SetTextAsync(textToCopy);
        }
    }

    protected abstract string? GetCopyableValue();
}