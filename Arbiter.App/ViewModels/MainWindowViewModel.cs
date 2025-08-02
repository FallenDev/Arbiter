using System;
using System.Net;
using System.Threading.Tasks;
using Arbiter.App.Models;
using Arbiter.App.Services;
using Arbiter.App.Views;
using Arbiter.Net;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace Arbiter.App.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private ArbiterSettings Settings { get; set; } = new();
    
    private readonly ILogger<MainWindowViewModel> _logger;
    private readonly IDialogService _dialogService;
    private readonly IGameClientService _gameClientService;
    private readonly ISettingsService _settingsService;
    
    private readonly ProxyServer _proxyServer = new();
    
    [ObservableProperty]
    private string _title = "Arbiter";
    
    public ConsoleViewModel Console { get; }

    public MainWindowViewModel(
        ILogger<MainWindowViewModel> logger,
        IDialogService dialogService,
        IGameClientService gameClientService,
        ISettingsService settingsService,
        ConsoleViewModel consoleViewModel)
    {
        Console = consoleViewModel;

        _logger = logger;
        _dialogService = dialogService;
        _gameClientService = gameClientService;
        _settingsService = settingsService;
        
        _ = LoadSettingsAsync();
        _ = StartProxyAsync();
    }

    private async Task LoadSettingsAsync()
    {
        Settings = await _settingsService.LoadFromFileAsync();
    }

    private async Task StartProxyAsync()
    {
        try
        {
            var remoteIpAddress = await Dns.GetHostAddressesAsync(Settings.RemoteServerAddress);
            if (remoteIpAddress.Length == 0)
            {
                throw new Exception("Failed to resolve remote server address");
            }

            _proxyServer.Start(Settings.LocalPort, remoteIpAddress[0], Settings.RemoteServerPort);

            var localEndpoint = _proxyServer.LocalEndpoint!;
            _logger.LogInformation("Proxy started on {IP}:{Port}", localEndpoint.Address, localEndpoint.Port);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start proxy server");
            await _dialogService.ShowMessageBoxAsync(new MessageBoxDetails
            {
                Title = "Failed to Start Proxy Server",
                Message = $"An error occurred while starting the proxy server:\n\n{ex.Message}",
                Description = "You can change the local and remote server in Settings."
            });
        }
    }

    [RelayCommand]
    private async Task ShowSettings()
    {
        var newSettings =
            await _dialogService.ShowDialogAsync<SettingsWindow, SettingsViewModel, ArbiterSettings>();

        if (newSettings is null)
        {
            return;
        }

        Settings = newSettings;
        await _settingsService.SaveToFileAsync(Settings);
    }

    [RelayCommand]
    private async Task LaunchClient()
    {
        try
        {
            var clientExecutablePath = Settings.ClientExecutablePath;
            await _gameClientService.LaunchLoopbackClient(clientExecutablePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to launch client");
            await _dialogService.ShowMessageBoxAsync(new MessageBoxDetails
            {
                Title = "Failed to Launch Client",
                Message = $"An error occurred while launching the client:\n\n{ex.Message}",
                Description = "You can change the client executable path in Settings."
            });
        }
    }
}



