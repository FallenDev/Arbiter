using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Arbiter.App.ViewModels.Dialogs;

public partial class DialogViewModel : ViewModelBase
{
    [ObservableProperty] private string? _name;
    [ObservableProperty] private string? _content;
    [ObservableProperty] private int? _sprite;
    [ObservableProperty] private int? _entityId;
    [ObservableProperty] private int? _pursuitId;
    [ObservableProperty] private int? _stepId;

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(NavigatePreviousCommand))]
    private bool _canNavigatePrevious;

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(NavigateNextCommand))]
    private bool _canNavigateNext;

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(NavigateTopCommand))]
    private bool _canNavigateTop;

    public ObservableCollection<DialogMenuChoiceViewModel> MenuChoices { get; } = [];

    public EventHandler<DialogEventArgs>? RequestPrevious;
    public EventHandler<DialogEventArgs>? RequestNext;
    public EventHandler<DialogEventArgs>? RequestTop;
    public EventHandler<DialogEventArgs>? RequestClose;
    public EventHandler<DialogMenuEventArgs>? MenuChoiceSelected;

    [RelayCommand]
    private void SelectMenuChoice(DialogMenuChoiceViewModel viewModel)
    {
        MenuChoiceSelected?.Invoke(this, new DialogMenuEventArgs(viewModel));
    }

    [RelayCommand(CanExecute = nameof(CanNavigatePrevious))]
    private void NavigatePrevious()
    {
        if (!CanNavigatePrevious)
        {
            return;
        }

        RequestPrevious?.Invoke(this, new DialogEventArgs());
    }

    [RelayCommand(CanExecute = nameof(CanNavigateNext))]
    private void NavigateNext()
    {
        if (!CanNavigateNext)
        {
            return;
        }

        RequestNext?.Invoke(this, new DialogEventArgs());
    }

    [RelayCommand(CanExecute = nameof(CanNavigateTop))]
    private void NavigateTop()
    {
        if (!CanNavigateTop)
        {
            return;
        }

        RequestTop?.Invoke(this, new DialogEventArgs());
    }

    [RelayCommand]
    private void CloseDialog()
    {
        RequestClose?.Invoke(this, new DialogEventArgs());
    }
}