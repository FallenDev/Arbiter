using CommunityToolkit.Mvvm.ComponentModel;

namespace Arbiter.App.ViewModels.Dialogs;

public partial class DialogMenuChoiceViewModel : ViewModelBase
{
    [ObservableProperty] private string _text = string.Empty;

    [ObservableProperty] 
    [NotifyPropertyChangedFor(nameof(IsModMenuChoice))]
    private int? _pursuitId;

    public required int Index { get; init; }

    public bool IsModMenuChoice => PursuitId >= 0xFF00;
}