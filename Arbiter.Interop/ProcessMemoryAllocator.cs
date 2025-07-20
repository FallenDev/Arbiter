using System.ComponentModel;
using Arbiter.Interop.Win32;

namespace Arbiter.Interop;

public class ProcessMemoryAllocator : IDisposable
{
    private bool _isDisposed;
    private readonly bool _leaveOpen;
    
    public IntPtr ProcessHandle { get; private set; }
    
    public ProcessMemoryAllocator(IntPtr processHandle, bool leaveOpen = false)
    {
        ProcessHandle = processHandle;
        _leaveOpen = leaveOpen;
    }

    ~ProcessMemoryAllocator()
    {
        Dispose(false);
    }

    public IntPtr AllocMemory(Action<BinaryWriter> initializer, long? minimumSize = null)
    {
        CheckIfDisposed();
        
        using var memoryStream = new MemoryStream();
        if (minimumSize is > 0)
        {
            memoryStream.SetLength(minimumSize.Value);
        }

        using var writer = new BinaryWriter(memoryStream);
        initializer(writer);
        memoryStream.Position = 0;

        var size = memoryStream.Length;

        var memPointer = NativeMethods.VirtualAllocEx(ProcessHandle, IntPtr.Zero, (UIntPtr)size,
            Win32AllocationType.Commit,
            Win32MemoryProtection.ReadWrite);

        if (memPointer == IntPtr.Zero)
        {
            throw new Win32Exception();
        }

        using var processMemoryStream =
            new ProcessMemoryStream(ProcessHandle, ProcessAccessFlags.ReadWrite, leaveOpen: true);
        processMemoryStream.Position = memPointer;
        memoryStream.CopyTo(processMemoryStream, 4096);

        return memPointer;
    }

    public void FreeMemory(IntPtr memPointer)
    {
        CheckIfDisposed();
        
        if (!NativeMethods.VirtualFreeEx(ProcessHandle, memPointer, UIntPtr.Zero, Win32FreeType.Release))
        {
            throw new Win32Exception();
        }
    }
    
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool isDisposing)
    {
        if (_isDisposed)
        {
            return;
        }

        if (isDisposing)
        {
            // Cleanup managed resources here
        }

        if (!_leaveOpen && ProcessHandle != IntPtr.Zero)
        {
            NativeMethods.CloseHandle(ProcessHandle);
        }

        ProcessHandle = IntPtr.Zero;
        _isDisposed = true;
    }

    private void CheckIfDisposed() => ObjectDisposedException.ThrowIf(_isDisposed, "Allocator is disposed");
}