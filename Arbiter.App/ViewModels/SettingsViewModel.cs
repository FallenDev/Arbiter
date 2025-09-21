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
    private static readonly FilePickerFileType ExecutableType = new("All Executables")
    {
        Patterns = ["*.exe"],
        MimeTypes = ["application/octet-stream"],
    };
    
    private readonly ISettingsService _settingsService;
    private readonly IStorageProvider _storageProvider;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ClientExecutablePath))]
    [NotifyPropertyChangedFor(nameof(LocalPort))]
    [NotifyPropertyChangedFor(nameof(RemoteServerAddress))]
    [NotifyPropertyChangedFor(nameof(RemoteServerPort))]
    [NotifyPropertyChangedFor(nameof(TraceOnStartup))]
    [NotifyPropertyChangedFor(nameof(TraceAutosave))]
    private ArbiterSettings _settings = new();
    
    [ObservableProperty] private bool _hasChanges;

    public string ClientExecutablePath
    {
        get => Settings.ClientExecutablePath;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ValidationException("Client executable path cannot be empty");
            }

            Settings.ClientExecutablePath = value;
            OnPropertyChanged();
            HasChanges = true;
        }
    }

    public int LocalPort
    {
        get => Settings.LocalPort;
        set
        {
            Settings.LocalPort = value;
            OnPropertyChanged();
            HasChanges = true;
        }
    }

    public string RemoteServerAddress
    {
        get => Settings.RemoteServerAddress;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ValidationException("Remote server address cannot be empty");
            }

            Settings.RemoteServerAddress = value;
            OnPropertyChanged();
            HasChanges = true;
        }
    }

    public int RemoteServerPort
    {
        get => Settings.RemoteServerPort;
        set
        {
            Settings.RemoteServerPort = value;
            OnPropertyChanged();
            HasChanges = true;
        }
    }

    public bool TraceOnStartup
    {
        get => Settings.TraceOnStartup;
        set
        {
            Settings.TraceOnStartup = value;
            OnPropertyChanged();
            HasChanges = true;
        }
    }

    public bool TraceAutosave
    {
        get => Settings.TraceAutosave;
        set
        {
            Settings.TraceAutosave = value;
            OnPropertyChanged();
            HasChanges = true;
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
        Settings = await _settingsService.LoadFromFileAsync();
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
        RequestClose?.Invoke(Settings);
    }

    [RelayCommand]
    private void HandleCancel()
    {
        RequestClose?.Invoke(null);
    }

    [RelayCommand]
    private void HandleResetDefaults()
    {
        Settings = new ArbiterSettings();
        HasChanges = true;
    }
}