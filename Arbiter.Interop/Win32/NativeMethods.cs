using System.Runtime.InteropServices;

namespace Arbiter.Interop.Win32;

internal static class NativeMethods
{
    [DllImport("kernel32.dll", EntryPoint = "CreateProcess", SetLastError = true, CharSet = CharSet.Unicode)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool CreateProcess(string applicationName,
        string? commandLine,
        ref Win32SecurityAttributes processAttributes,
        ref Win32SecurityAttributes threadAttributes,
        bool inheritHandles,
        Win32ProcessCreationFlags creationFlags,
        IntPtr environment,
        string? currentDirectory,
        ref Win32StartupInfo startupInfo,
        out Win32ProcessInformation processInformation);

    [DllImport("kernel32.dll", EntryPoint = "ResumeThread", SetLastError = true)]
    internal static extern uint ResumeThread(IntPtr hThread);

    [DllImport("kernel32.dll", EntryPoint = "ReadProcessMemory", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr baseAddress, byte[] buffer, int size,
        out IntPtr bytesRead);

    [DllImport("kernel32.dll", EntryPoint = "WriteProcessMemory", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr baseAddress, byte[] buffer, int size,
        out IntPtr bytesWritten);

    [DllImport("kernel32.dll", EntryPoint = "VirtualAllocEx", SetLastError = true)]
    internal static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr baseAddress, UIntPtr desiredSize,
        Win32AllocationType allocationType, Win32MemoryProtection protect);

    [DllImport("kernel32.dll", EntryPoint = "VirtualProtectEx", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool VirtualProtectEx(IntPtr hProcess, IntPtr baseAddress, UIntPtr size,
        Win32MemoryProtection newProtect, out Win32MemoryProtection oldProtect);

    [DllImport("kernel32.dll", EntryPoint = "VirtualFreeEx", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr baseAddress, UIntPtr size,
        Win32FreeType freeType);

    [DllImport("kernel32.dll", EntryPoint = "TerminateProcess", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool TerminateProcess(IntPtr hProcess, uint exitCode);

    [DllImport("kernel32.dll", EntryPoint = "OpenProcess", SetLastError = true)]
    internal static extern IntPtr OpenProcess(Win32ProcessAccess desireAccess, bool inheritHandle, int processId);

    [DllImport("kernel32.dll", EntryPoint = "CloseHandle", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool CloseHandle(IntPtr handle);

    [DllImport("user32.dll", EntryPoint = "GetClassLongPtr")]
    internal static extern IntPtr GetClassLongPtr(IntPtr hWindow, Win32GetClassLongIndex index);

    [DllImport("user32.dll", EntryPoint = "SetClassLongPtr")]
    internal static extern IntPtr SetClassLongPtr(IntPtr hWindow, Win32GetClassLongIndex index, IntPtr newValue);
}