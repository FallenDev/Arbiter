using System;
using System.Net;
using System.Threading.Tasks;
using Arbiter.App.Models;
using Arbiter.App.Services;
using Arbiter.App.ViewModels.Client;
using Arbiter.App.ViewModels.Inspector;
using Arbiter.App.ViewModels.Logging;
using Arbiter.App.ViewModels.Tracing;
using Arbiter.App.Views;
using Avalonia;
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
    public ConsoleViewModel Console { get; }
    public InspectorViewModel Inspector { get; }
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
        Console = serviceProvider.GetRequiredService<ConsoleViewModel>();
        Inspector = serviceProvider.GetRequiredService<InspectorViewModel>();
        Proxy = serviceProvider.GetRequiredService<ProxyViewModel>();
        Trace = serviceProvider.GetRequiredService<TraceViewModel>();

        Trace.SelectedPacketChanged += OnPacketSelected;
    }

    private void OnPacketSelected(TracePacketViewModel? viewModel)
    {
        if (viewModel is null)
        {
            SelectedRawHex = null;
            Inspector.SelectedPacket = null;
            return;
        }

        SelectedRawHex = new RawHexViewModel(viewModel.DecryptedPacket);
        SelectedRawHex.ClearSelection();

        Inspector.SelectedPacket = viewModel.DecryptedPacket;
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
    }

    private bool CanLaunchClient() =>
        !string.IsNullOrWhiteSpace(Settings.ClientExecutablePath) && OperatingSystem.IsWindows();

    [RelayCommand(CanExecute = nameof(CanLaunchClient))]
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
            _logger.LogInformation("Proxy started on 127.0.0.1:{Port}", Settings.LocalPort);
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

    internal async Task OnOpened()
    {
        Settings = await _settingsService.LoadFromFileAsync();
        LaunchClientCommand.NotifyCanExecuteChanged();

        if (Settings.StartupLocation is not null)
        {
            RestoreWindowPosition(Settings.StartupLocation);
        }
    }

    internal async Task OnLoaded()
    {
        await StartProxyAsync();

        if (Settings.TraceOnStartup)
        {
            Trace.StartTracing();
        }
    }

    internal async Task<bool> OnClosing(WindowCloseReason reason)
    {
        await SaveWindowPositionAsync();
        
        if (Trace.IsRunning)
        {
            Trace.StopTracing();
        }

        return true;
    }

    private async Task SaveWindowPositionAsync()
    {
        Settings.StartupLocation = new WindowRect
        {
            X = _mainWindow.Position.X,
            Y = _mainWindow.Position.Y,
            Width = (int)_mainWindow.Width,
            Height = (int)_mainWindow.Height,
            IsMaximized = _mainWindow.WindowState == WindowState.Maximized
        };

        await _settingsService.SaveToFileAsync(Settings);
    }

    private void RestoreWindowPosition(WindowRect rect)
    {
        if (rect is { X: >= 0, Y: >= 0 })
        {
            _mainWindow.Position = new PixelPoint(rect.X.Value, rect.Y.Value);
        }

        if (rect.Width is > 0)
        {
            _mainWindow.Width = rect.Width.Value;
        }

        if (rect.Height is > 0)
        {
            _mainWindow.Height = rect.Height.Value;       
        }

        _mainWindow.WindowState = rect.IsMaximized ? WindowState.Maximized : WindowState.Normal;
    }
}



