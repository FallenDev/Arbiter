using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input.Platform;
using Avalonia.VisualTree;

namespace Arbiter.App.Extensions;

public static class ApplicationExtensions
{
    public static IClipboard? TryGetClipboard(this Application app)
    {
        if (app.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            return desktop.MainWindow?.Clipboard;
        }

        if (app.ApplicationLifetime is ISingleViewApplicationLifetime singleView)
        {
            var visualRoot = singleView.MainView?.GetVisualRoot();
            if (visualRoot is TopLevel topLevel)
            {
                return topLevel.Clipboard;
            }
        }

        return null;
    }
}