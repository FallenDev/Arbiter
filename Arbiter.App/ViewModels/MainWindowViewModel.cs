using System;
using System.Net;
using System.Threading.Tasks;
using Arbiter.App.Models;
using Arbiter.App.Services;
using Arbiter.App.ViewModels.Client;
using Arbiter.App.ViewModels.Inspector;
using Arbiter.App.ViewModels.Logging;
using Arbiter.App.ViewModels.Proxy;
using Arbiter.App.ViewModels.Tracing;
using Arbiter.App.Views;
using Arbiter.Net.Filters;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Arbiter.App.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private ArbiterSettings Settings { get; set; } = new();

    private readonly ILogger<MainWindowViewModel> _logger;
    private readonly IDialogService _dialogService;
    private readonly IGameClientService _gameClientService;
    private readonly ISettingsService _settingsService;
    private readonly Window _mainWindow;

    [ObservableProperty] private string _title = "Arbiter";
    [ObservableProperty] private ClientViewModel? _selectedClient;
    [ObservableProperty] private RawHexViewModel? _selectedRawHex;

    public ClientManagerViewModel ClientManager { get; }
    public SendPacketViewModel SendPacket { get; }
    public ConsoleViewModel Console { get; }
    public InspectorViewModel Inspector { get; }
    public CrcCalculatorViewModel CrcCalculator { get; }
    public ProxyViewModel Proxy { get; }
    public TraceViewModel Trace { get; }

    public MainWindowViewModel(
        ILogger<MainWindowViewModel> logger,
        IServiceProvider serviceProvider,
        IDialogService dialogService,
        IGameClientService gameClientService,
        ISettingsService settingsService,
        Window mainWindow)
    {
        _logger = logger;

        _dialogService = dialogService;
        _gameClientService = gameClientService;
        _settingsService = settingsService;
        _mainWindow = mainWindow;

        ClientManager = serviceProvider.GetRequiredService<ClientManagerViewModel>();
        SendPacket = serviceProvider.GetRequiredService<SendPacketViewModel>();
        Console = serviceProvider.GetRequiredService<ConsoleViewModel>();
        Inspector = serviceProvider.GetRequiredService<InspectorViewModel>();
        CrcCalculator = serviceProvider.GetRequiredService<CrcCalculatorViewModel>();
        Proxy = serviceProvider.GetRequiredService<ProxyViewModel>();
        Trace = serviceProvider.GetRequiredService<TraceViewModel>();

        Trace.SelectedPacketChanged += OnPacketSelected;
    }

    private bool CanLaunchClient() =>
        !string.IsNullOrWhiteSpace(Settings.ClientExecutablePath) && OperatingSystem.IsWindows();

    [RelayCommand(CanExecute = nameof(CanLaunchClient))]
    private async Task LaunchClient()
    {
        try
        {
            var clientExecutablePath = Settings.ClientExecutablePath;
            var options = new LaunchClientOptions(Settings.LocalPort, Settings.SkipIntroVideo,
                Settings.SuppressLoginNotice);

            await _gameClientService.LaunchLoopbackClient(clientExecutablePath, options);
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

    private async Task StartProxyAsync()
    {
        try
        {
            var remoteIpAddress = await Dns.GetHostAddressesAsync(Settings.RemoteServerAddress);
            if (remoteIpAddress.Length == 0)
            {
                throw new Exception("Failed to resolve remote server address");
            }

            Proxy.Start(Settings.LocalPort, remoteIpAddress[0], Settings.RemoteServerPort);
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

    private void OnPacketSelected(TracePacketViewModel? viewModel)
    {
        if (viewModel is null)
        {
            SelectedRawHex = null;
            Inspector.SelectedPacket = null;
            return;
        }

        SelectedRawHex = new RawHexViewModel(viewModel);
        SelectedRawHex.ClearSelection();

        Inspector.SelectedPacket = viewModel;
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
        LaunchClientCommand.NotifyCanExecuteChanged();

        ApplySettings();
    }

    private void ApplySettings()
    {
        Proxy.ApplyDebugFilters(Settings.Debug, Settings.MessageFilters);
        Trace.MaxTraceHistory = Settings.TraceMaxHistory;
    }
}
