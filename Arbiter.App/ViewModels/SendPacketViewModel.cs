using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Arbiter.App.ViewModels.Client;
using Arbiter.Net.Proxy;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Arbiter.App.ViewModels;

public partial class SendPacketViewModel : ViewModelBase
{
    private readonly ILogger<SendPacketViewModel> _logger;
    private readonly ProxyServer _proxyServer;
    private readonly ClientManagerViewModel _clientManager;

    [ObservableProperty] private string _inputText = string.Empty;
    [ObservableProperty] private bool _hasClients;

    [ObservableProperty] private ClientViewModel? _selectedClient;
    [ObservableProperty] private TimeSpan _selectedDelay = TimeSpan.Zero;
    [ObservableProperty] private TimeSpan _selectedRate = TimeSpan.FromMilliseconds(100);

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(StartSendCommand))]
    [NotifyCanExecuteChangedFor(nameof(CancelSendCommand))]
    private bool _isSending;

    public ObservableCollection<ClientViewModel> Clients => _clientManager.Clients;

    public ObservableCollection<TimeSpan> AvailableDelays =>
    [
        TimeSpan.Zero,
        TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(4),
        TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(15), TimeSpan.FromSeconds(30)
    ];

    public ObservableCollection<TimeSpan> AvailableRates =>
    [
        TimeSpan.Zero,
        TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(200), TimeSpan.FromMilliseconds(300),
        TimeSpan.FromMilliseconds(400), TimeSpan.FromMilliseconds(500), TimeSpan.FromSeconds(1),
        TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(4),
        TimeSpan.FromSeconds(5)
    ];

    public SendPacketViewModel(ILogger<SendPacketViewModel> logger,
        IServiceProvider serviceProvider, ProxyServer proxyServer)
    {
        _logger = logger;
        _proxyServer = proxyServer;
        _clientManager = serviceProvider.GetRequiredService<ClientManagerViewModel>();

        _clientManager.Clients.CollectionChanged += OnClientsCollectionChanged;
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
        if (SelectedClient is not null && !collection.Contains(SelectedClient))
        {
            CancelSend();
            SelectedClient = null;
        }
    }

    private bool CanSend() => !IsSending;

    [RelayCommand(CanExecute = nameof(CanSend))]
    public void StartSend()
    {
        if (IsSending)
        {
            return;
        }

        IsSending = true;
        CancelSendCommand.NotifyCanExecuteChanged();
    }

    private bool CanCancelSend() => IsSending;

    [RelayCommand(CanExecute = nameof(CanCancelSend))]
    public void CancelSend()
    {
        IsSending = false;
        CancelSendCommand.NotifyCanExecuteChanged();
    }

}