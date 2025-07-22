using System;
using System.Threading.Tasks;
using Arbiter.App.Models;
using Arbiter.App.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Arbiter.App.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IDialogService _dialogService;
    private readonly IGameClientService _gameClientService;
    
    [ObservableProperty]
    private string _title = "Arbiter";

    [ObservableProperty]
    private SettingsViewModel _settings;

    public MainWindowViewModel(IDialogService dialogService, IGameClientService gameClientService,
        SettingsViewModel settingsViewModel)
    {
        _dialogService = dialogService;
        _gameClientService = gameClientService;
        _settings = settingsViewModel;
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
            await _dialogService.ShowMessageBoxAsync(new MessageBoxDetails
            {
                Title = "Failed to Launch Client",
                Description = ex.Message
            });
        }
    }
}



