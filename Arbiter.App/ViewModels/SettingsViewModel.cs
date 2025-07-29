using System;
using System.ComponentModel.DataAnnotations;
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
    private ArbiterSettings _settings = new();
    private readonly ISettingsService _settingsService;
    private readonly IStorageProvider _storageProvider;
    
    private static readonly FilePickerFileType ExecutableType = new("All Executables")
    {
        Patterns = ["*.exe"],
        MimeTypes = ["application/octet-stream"],
    };

    [ObservableProperty] private bool _hasChanges;
    
    public string ClientExecutablePath
    {
        get => _settings.ClientExecutablePath;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ValidationException("Client executable path cannot be empty");
            }
            
            _settings.ClientExecutablePath = value;
            OnPropertyChanged();
            MarkDirty();
        }
    }
    
    public string RemoteServerAddress
    {
        get => _settings.RemoteServerAddress;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ValidationException("Remote server address cannot be empty");
            }
            
            _settings.RemoteServerAddress = value;
            OnPropertyChanged();
            MarkDirty();
        }
    }

    public int RemoteServerPort
    {
        get => _settings.RemoteServerPort;
        set
        {
            _settings.RemoteServerPort = value;
            OnPropertyChanged();
            MarkDirty();
        }
    }

    public SettingsViewModel(ISettingsService settingsService, IStorageProvider storageProvider)
    {
        _settingsService = settingsService;
        _storageProvider = storageProvider;

        _ = LoadSettingsAsync();
    }

    public event Action<ArbiterSettings?>? RequestClose;

    private async Task LoadSettingsAsync()
    {
        _settings = await _settingsService.LoadFromFileAsync();
        OnPropertyChanged(nameof(ClientExecutablePath));
        OnPropertyChanged(nameof(RemoteServerAddress));
        OnPropertyChanged(nameof(RemoteServerPort));
    }
    
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

    private void MarkDirty()
    {
        HasChanges = true;
        OnPropertyChanged(nameof(HasChanges));
    }
}