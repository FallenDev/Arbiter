using System.Diagnostics;
using System.IO;
using Path = System.IO.Path;

namespace Arbiter.App;

public static class AppHelper
{
    public static string GetStartupLocation()
    {
        var process = Process.GetCurrentProcess();
        var mainModule = process.MainModule;
        var filename = mainModule?.FileName;

        return string.IsNullOrWhiteSpace(filename)
            ? process.StartInfo.WorkingDirectory
            : Path.GetDirectoryName(filename) ?? string.Empty;
    }

    public static string GetRelativePath(params string[] components)
    {
        var startupPath = GetStartupLocation();
        return Path.Combine(startupPath, Path.Combine(components));
    }

    public static bool TryCreateDirectory(string directoryPath)
    {
        if (Directory.Exists(directoryPath))
        {
            return true;
        }

        try
        {
            Directory.CreateDirectory(directoryPath);
            return true;
        }
        catch
        {
            return false;
        }
    }
}