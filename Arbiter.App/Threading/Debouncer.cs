using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;

namespace Arbiter.App.Threading;

public sealed class Debouncer : IDisposable
{
    private bool _isDisposed;

    private readonly TimeSpan _delay;
    private readonly Dispatcher? _dispatcher;
    private CancellationTokenSource? _cts;

    public Debouncer(TimeSpan delay, Dispatcher? dispatcher = null)
    {
        _delay = delay;
        _dispatcher = dispatcher;
    }

    public void Execute(Action action)
    {
        ThrowIfDisposed();

        var cts = ReplaceToken();
        var token = cts.Token;

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(_delay, token).ConfigureAwait(false);
                if (token.IsCancellationRequested)
                {
                    return;
                }

                if (_dispatcher is not null)
                {
                    await _dispatcher.InvokeAsync(action);
                }
                else
                {
                    action();
                }
            }
            catch (OperationCanceledException)
            {
                // do nothing, probably superseded by a newer call
            }
        }, token);
    }

    public void Execute(Func<Task> actionAsync)
    {
        ThrowIfDisposed();

        var cts = ReplaceToken();
        var token = cts.Token;

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(_delay, token).ConfigureAwait(false);
                if (token.IsCancellationRequested)
                {
                    return;
                }

                if (_dispatcher is not null)
                {
                    await _dispatcher.InvokeAsync(async () => await actionAsync().ConfigureAwait(false));
                }
                else
                {
                    await actionAsync().ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                // do nothing, probably superseded by a newer call
            }
        }, token);
    }

    private CancellationTokenSource ReplaceToken()
    {
        var previousCts = Interlocked.Exchange(ref _cts, new CancellationTokenSource());
        try
        {
            previousCts?.Cancel();
        }
        finally
        {
            previousCts?.Dispose();
        }

        return _cts!;
    }

    private void ThrowIfDisposed()
    {
        if (_isDisposed)
        {
            throw new ObjectDisposedException(nameof(Debouncer));
        }
    }

    public void Dispose()
    {
        Dispose(true);
    }

    private void Dispose(bool isDisposing)
    {
        if (_isDisposed)
        {
            return;
        }

        if (isDisposing)
        {
            var cts = Interlocked.Exchange(ref _cts, null);
            cts?.Cancel();
            cts?.Dispose();
        }

        _isDisposed = true;
    }
}
