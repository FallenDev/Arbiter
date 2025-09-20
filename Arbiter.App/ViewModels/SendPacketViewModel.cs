using Arbiter.Net.Proxy;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Arbiter.App.ViewModels;

public partial class SendPacketViewModel : ViewModelBase
{
    private readonly ProxyServer _proxyServer;

    [ObservableProperty]
    private string _inputText = string.Empty;
    
    public SendPacketViewModel(ProxyServer proxyServer)
    {
        _proxyServer = proxyServer;
    }
}