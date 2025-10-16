using System;
using System.Collections.ObjectModel;
using Arbiter.App.ViewModels.Client;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Arbiter.App.ViewModels.Dialogs;

public partial class DialogViewModel : ViewModelBase
{
    private readonly ILogger<DialogViewModel> _logger;
    private readonly ClientManagerViewModel _clientManager;
    
    [ObservableProperty] private int? _dialogId;
    
    [ObservableProperty] private string? _name;
    [ObservableProperty] private string? _content;
    [ObservableProperty] private int? _sprite;
    [ObservableProperty] private int? _entityId;
    [ObservableProperty] private int? _pursuitId;
    [ObservableProperty] private int? _stepId;
    
    [ObservableProperty]
    private ClientViewModel? _selectedClient;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(NavigatePreviousCommand))]
    private bool _canNavigatePrevious;
    
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(NavigateNextCommand))]
    private bool _canNavigateNext;
    
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(NavigateTopCommand))]
    private bool _canNavigateTop;

    public ObservableCollection<ClientViewModel> Clients => _clientManager.Clients;
    
    public ObservableCollection<DialogMenuChoiceViewModel> MenuChoices { get; } = [];

    public DialogViewModel(ILogger<DialogViewModel> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _clientManager = serviceProvider.GetRequiredService<ClientManagerViewModel>();
    }

    [RelayCommand]
    private void SelectMenuChoice(DialogMenuChoiceViewModel viewModel)
    {
        
    }
    
    [RelayCommand(CanExecute = nameof(CanNavigatePrevious))]
    private void NavigatePrevious()
    {
        if (!CanNavigatePrevious)
        {
            return;
        }
    }

    [RelayCommand(CanExecute = nameof(CanNavigateNext))]
    private void NavigateNext()
    {
        if (!CanNavigateNext)
        {
            return;
        }   
    }

    [RelayCommand(CanExecute = nameof(CanNavigateTop))]
    private void NavigateTop()
    {
        if (!CanNavigateTop)
        {
            return;
        }
    }
    
    [RelayCommand]
    private void CloseDialog()
    {
        
    }
    
    [RelayCommand]
    private void LoadDialog()
    {
        
    }

    [RelayCommand]
    private void SaveDialog()
    {
        
    }
}