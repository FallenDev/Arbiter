using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Arbiter.App.ViewModels.Client;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Arbiter.App.ViewModels.Dialogs;

public partial class DialogManagerViewModel : ViewModelBase
{
    private readonly ILogger<DialogManagerViewModel> _logger;
    private readonly ClientManagerViewModel _clientManager;
    
    [ObservableProperty] private DialogViewModel? _activeDialog;
    
    [ObservableProperty]
    private bool _hasClients;
    
    [ObservableProperty]
    private ClientViewModel? _selectedClient;

    public ObservableCollection<ClientViewModel> Clients => _clientManager.Clients;

    public DialogManagerViewModel(ILogger<DialogManagerViewModel> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _clientManager = serviceProvider.GetRequiredService<ClientManagerViewModel>();
        
        _clientManager.Clients.CollectionChanged += OnClientsCollectionChanged;
        _clientManager.ClientSelected += OnClientSelected;
    }
    
    private void OnClientSelected(ClientViewModel? client)
    {
        if (client is null || SelectedClient is not null)
        {
            return;
        }

        // Automatically select the client if none is selected
        SelectedClient = client;
    }

    private void OnClientsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (sender is not ObservableCollection<ClientViewModel> collection)
        {
            return;
        }

        HasClients = collection.Count > 0;

        if (e.Action != NotifyCollectionChangedAction.Remove)
        {
            return;
        }

        // If the currently selected client was removed from the collection, clear the selection
        if (SelectedClient is null || collection.Contains(SelectedClient))
        {
            return;
        }
        
        SelectedClient = null;
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