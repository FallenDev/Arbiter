using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Arbiter.App.Models;
using Arbiter.Json.Converters;
using Avalonia.Platform.Storage;

using Path = System.IO.Path;

namespace Arbiter.App.Services;

public class TraceService : ITraceService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = true,
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, false),
            new HexNumberConverterFactory(),
            new HexByteEnumerableConverterFactory()
        }
    };

    private readonly IStorageProvider _storageProvider;

    public TraceService(IStorageProvider storageProvider)
    {
        _storageProvider = storageProvider;
    }

    public async Task<TraceFile> LoadTraceFileAsync(string filePath)
    {
        var directoryName = Path.GetDirectoryName(filePath) ?? string.Empty;
        var directory = await _storageProvider.TryGetFolderFromPathAsync(directoryName);

        if (directory is null)
        {
            throw new DirectoryNotFoundException($"Directory does not exist: {directoryName}");
        }

        var filename = Path.GetFileName(filePath);
        var file = await directory.GetFileAsync(filename);

        if (file is null)
        {
            throw new FileNotFoundException($"File does not exist: {filename}", filePath);
        }

        await using var stream = await file.OpenReadAsync();
        var deserialized = await JsonSerializer.DeserializeAsync<TraceFile>(stream, JsonOptions);

        return deserialized!;
    }

    public async Task SaveTraceFileAsync(TraceFile trace, string filePath)
    {
        var directoryName = Path.GetDirectoryName(filePath) ?? string.Empty;
        var directory = await _storageProvider.TryGetFolderFromPathAsync(directoryName);

        if (directory is null)
        {
            throw new DirectoryNotFoundException($"Directory does not exist: {directoryName}");
        }

        var filename = Path.GetFileName(filePath);
        using var file = await directory.CreateFileAsync(filename);

        await using var stream = await file!.OpenWriteAsync();
        await using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true });

        await JsonSerializer.SerializeAsync(stream, trace, JsonOptions);
    }
}