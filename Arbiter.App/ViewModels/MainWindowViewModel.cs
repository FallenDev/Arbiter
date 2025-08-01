using System;
using System.Threading.Tasks;
using Arbiter.App.Models;
using Arbiter.App.Services;
using Arbiter.App.Views;
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
        
        AddMockLogLines();
    }

    private async Task LoadSettingsAsync()
    {
        Settings = await _settingsService.LoadFromFileAsync();
    }

    private void AddMockLogLines()
    {
        for (var i = 0; i < 6; i++)
        {
            _logger.LogInformation("This is a mock log line ({i})", i + 1);
        }

        for (var i = 0; i < 5; i++)
        {
            _logger.LogWarning("This is a mock warning ({i})", i + 1);
        }

        for (var i = 0; i < 4; i++)
        {
            _logger.LogError("This is a mock error ({i})", i + 1);
        }
        
        for (var i = 0; i < 3; i++)
        {
            _logger.LogDebug("This is a mock debug message ({i})", i + 1);
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



