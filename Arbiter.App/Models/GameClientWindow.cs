using System;
using Arbiter.Interop.Window;

namespace Arbiter.App.Models;

public class GameClientWindow
{
    public int ProcessId { get; set; }
    public IntPtr WindowHandle { get; set; }
    public string WindowClassName { get; set; } = string.Empty;
    public string WindowTitle { get; set; } = string.Empty;
    public string? CharacterName { get; set; }

    public void SetWindowTitle(string title)
    {
        NativeWindowHelper.SetWindowTitle(WindowHandle, title);
        WindowTitle = title;
    }

    public void BringToFront()
    {
        NativeWindowHelper.BringToFront(WindowHandle);
    }
}