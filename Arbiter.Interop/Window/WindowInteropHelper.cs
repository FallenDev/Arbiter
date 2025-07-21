using Arbiter.Interop.Win32;

namespace Arbiter.Interop.Window;

public static class WindowInteropHelper
{
    public static IntPtr GetWindowStyle(IntPtr windowHandle) =>
        NativeMethods.GetClassLongPtr(windowHandle, Win32GetClassLongIndex.WindowStyle);

    public static IntPtr SetWindowStyle(IntPtr windowHandle, IntPtr windowStyle)
        => NativeMethods.SetClassLongPtr(windowHandle, Win32GetClassLongIndex.WindowStyle, windowStyle);
}