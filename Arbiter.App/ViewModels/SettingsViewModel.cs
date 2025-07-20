using System;
using System.IO;
using System.Threading.Tasks;
using Arbiter.App.Models;
using Arbiter.App.Services;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Arbiter.App.ViewModels;

public partial class SettingsViewModel : ViewModelBase, IDialogResult<ArbiterSettings>
{
    private static readonly string DefaultPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "KRU",
        "Dark Ages");

    private static readonly FilePickerFileType ExecutableType = new("All Executables")
    {
        Patterns = ["*.exe"],
        MimeTypes = ["application/octet-stream"],
    };

    private readonly IStorageProvider _storageProvider;
    private readonly ISettingsService _settingsService;

    [ObservableProperty] private string _clientExecutablePath = string.Empty;

    public event Action<ArbiterSettings?>? RequestClose;

    public SettingsViewModel(IStorageProvider storageProvider, ISettingsService settingsService)
    {
        _storageProvider = storageProvider;
        _settingsService = settingsService;

        _ = LoadSettingsAsync();
    }

    private async Task LoadSettingsAsync()
    {
        var settings = await _settingsService.LoadSettingsAsync();
        ClientExecutablePath = settings.ClientExecutablePath;
    }

    [RelayCommand]
    private async Task OnLocateClient()
    {
        var existingFolder =
            await _storageProvider.TryGetFolderFromPathAsync(
                Path.GetDirectoryName(ClientExecutablePath) ?? DefaultPath);

        var files = await _storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select Client Executable",
            FileTypeFilter =
            [
                ExecutableType,
            ],
            SuggestedFileName = "Darkages.exe",
            SuggestedStartLocation = existingFolder,
            AllowMultiple = false
        });

        var selectedFile = files.Count > 0 ? files[0] : null;

        if (selectedFile is not null)
        {
            ClientExecutablePath = selectedFile.TryGetLocalPath() ?? string.Empty;
        }
    }

    [RelayCommand]
    private void HandleOk()
    {
        RequestClose?.Invoke(new ArbiterSettings
        {
            ClientExecutablePath = ClientExecutablePath
        });
    }

    [RelayCommand]
    private void HandleCancel()
    {
        RequestClose?.Invoke(null);
    }
}