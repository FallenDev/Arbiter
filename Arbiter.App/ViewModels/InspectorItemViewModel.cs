using System.Threading.Tasks;
using Arbiter.App.Extensions;
using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Arbiter.App.ViewModels;

public abstract partial class InspectorItemViewModel : ViewModelBase
{
    private string _name = string.Empty;
    private int _order = int.MaxValue;

    [ObservableProperty] private string? _toolTip;
    
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public int Order
    {
        get => _order;
        set => SetProperty(ref _order, value);
    }

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