using System;
using System.Collections.ObjectModel;
using Arbiter.App.ViewModels.Client;
using Arbiter.Net.Proxy;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Arbiter.App.ViewModels;

public partial class SendPacketViewModel : ViewModelBase
{
    private readonly ILogger<SendPacketViewModel> _logger;
    private readonly ProxyServer _proxyServer;
    private readonly ClientManagerViewModel _clientManager;

    [ObservableProperty] private string _inputText = string.Empty;
    
    public ObservableCollection<ClientViewModel> Clients => _clientManager.Clients;
    
    public ClientViewModel? SelectedClient { get; set; }

    public SendPacketViewModel(ILogger<SendPacketViewModel> logger,
        IServiceProvider serviceProvider, ProxyServer proxyServer)
    {
        _logger = logger;
        _proxyServer = proxyServer;
        _clientManager = serviceProvider.GetRequiredService<ClientManagerViewModel>();
    }
}