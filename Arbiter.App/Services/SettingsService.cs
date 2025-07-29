using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Arbiter.App.Models;
using Avalonia.Platform.Storage;

namespace Arbiter.App.Services;

public class SettingsService : ISettingsService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
        },
        WriteIndented = true
    };

    // Default to the folder where the application is running
    private static readonly string SettingsDirectory =
        Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;

    private const string SettingsFileName = "settings.json";

    private readonly IStorageProvider _storageProvider;

    public SettingsService(IStorageProvider storageProvider)
    {
        _storageProvider = storageProvider;
    }

    public async Task<ArbiterSettings> LoadFromFileAsync()
    {
        var settingsFolder = await _storageProvider.TryGetFolderFromPathAsync(SettingsDirectory);
        if (settingsFolder is null)
        {
            return new ArbiterSettings();
        }

        var settingsFile = await settingsFolder.GetFileAsync(SettingsFileName);
        if (settingsFile is null)
        {
            return new ArbiterSettings();
        }

        await using var stream = await settingsFile.OpenReadAsync();
        var settings = await JsonSerializer.DeserializeAsync<ArbiterSettings>(stream, JsonOptions);

        return settings ?? new ArbiterSettings();
    }

    public async Task SaveToFileAsync(ArbiterSettings settings)
    {
        var settingsFolder = await _storageProvider.TryGetFolderFromPathAsync(SettingsDirectory);
        if (settingsFolder is null)
        {
            throw new FileNotFoundException("Settings directory does not exist", SettingsDirectory);
        }

        var settingsFile = await settingsFolder.CreateFileAsync(SettingsFileName);
        await using var stream = await settingsFile!.OpenWriteAsync();

        await JsonSerializer.SerializeAsync(stream, settings, JsonOptions);
    }
}