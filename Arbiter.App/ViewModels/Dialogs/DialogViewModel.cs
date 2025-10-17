﻿using System;
using System.Collections.ObjectModel;
using Arbiter.Net.Types;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Arbiter.App.ViewModels.Dialogs;

public partial class DialogViewModel : ViewModelBase
{
    [ObservableProperty] private string? _name;
    [ObservableProperty] private string? _content;
    [ObservableProperty] private int? _sprite;
    [ObservableProperty] private long? _entityId;
    [ObservableProperty] private EntityTypeFlags _entityType;
    [ObservableProperty] private int? _pursuitId;
    [ObservableProperty] private int? _stepId;
    [ObservableProperty] private bool _isTextInput;
    [ObservableProperty] private string? _promptLine1;
    [ObservableProperty] private string? _promptLine2;

    [ObservableProperty] private int _inputMaxLength = 255;

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(ConfirmTextInputCommand))]
    private string? _inputText;

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(NavigatePreviousCommand))]
    private bool _canNavigatePrevious;

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(NavigateNextCommand))]
    private bool _canNavigateNext;

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(NavigateTopCommand))]
    private bool _canNavigateTop;

    public ObservableCollection<DialogMenuChoiceViewModel> MenuChoices { get; } = [];

    public bool HasMenuChoices => MenuChoices.Count > 0;

    public event EventHandler<DialogEventArgs>? RequestPrevious;
    public event EventHandler<DialogEventArgs>? RequestNext;
    public event EventHandler<DialogEventArgs>? RequestTop;
    public event EventHandler<DialogEventArgs>? RequestClose;
    public event EventHandler<DialogMenuEventArgs>? MenuChoiceSelected;
    public event EventHandler<DialogEventArgs>? TextInputConfirmed;

    public DialogViewModel()
    {
        MenuChoices.CollectionChanged += (_, _) => { OnPropertyChanged(nameof(HasMenuChoices)); };
    }

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

    private bool CanConfirmTextInput() => IsTextInput && !string.IsNullOrWhiteSpace(InputText);

    [RelayCommand(CanExecute = nameof(CanConfirmTextInput))]
    private void ConfirmTextInput()
    {
        if (!IsTextInput)
        {
            return;
        }

        TextInputConfirmed?.Invoke(this, new DialogEventArgs());
    }

    [RelayCommand]
    private void CloseDialog()
    {
        RequestClose?.Invoke(this, new DialogEventArgs());
    }
}