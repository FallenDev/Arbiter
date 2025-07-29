using System;
using System.IO;
using System.Threading.Tasks;
using Arbiter.App.Models;
using Arbiter.App.Services;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;

namespace Arbiter.App.ViewModels;

public partial class SettingsViewModel : ViewModelBase, IDialogResult<ArbiterSettings>
{
    private readonly ArbiterSettings _settings;
    private readonly IStorageProvider _storageProvider;

    private static readonly FilePickerFileType ExecutableType = new("All Executables")
    {
        Patterns = ["*.exe"],
        MimeTypes = ["application/octet-stream"],
    };

    public string ClientExecutablePath
    {
        get => _settings.ClientExecutablePath;
        set
        {
            _settings.ClientExecutablePath = value;
            OnPropertyChanged();
        }
    }

    public string RemoteServerAddress
    {
        get => _settings.RemoteServerAddress;
        set
        {
            _settings.RemoteServerAddress = value;
            OnPropertyChanged();
        }
    }

    public int RemoteServerPort
    {
        get => _settings.RemoteServerPort;
        set
        {
            _settings.RemoteServerPort = value;
            OnPropertyChanged();
        }
    }

    public SettingsViewModel(ISettingsService settingsService, IStorageProvider storageProvider)
    {
        _storageProvider = storageProvider;

        // Make a copy so we can handle apply/cancel
        _settings = (ArbiterSettings)settingsService.CurrentSettings.Clone();
    }

    public event Action<ArbiterSettings?>? RequestClose;

    [RelayCommand]
    private async Task OnLocateClient()
    {
        var existingFolder =
            await _storageProvider.TryGetFolderFromPathAsync(
                Path.GetDirectoryName(ClientExecutablePath) ?? ArbiterSettings.DefaultPath);

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
        RequestClose?.Invoke(_settings);
    }

    [RelayCommand]
    private void HandleCancel()
    {
        RequestClose?.Invoke(null);
    }
}