using System.Buffers;
using System.Collections.Concurrent;
using System.Threading.Channels;
using Arbiter.Net.Proxy;

namespace Arbiter.Net.Observers;

public class NetworkObserverDispatcher : IDisposable
{
    private record ObserverMessage(ProxyConnection Connection, INetworkMessage Message);

    private readonly ChannelWriter<ObserverMessage> _queueWriter;
    private readonly ChannelReader<ObserverMessage> _queueReader;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly Task _dispatcherTask;

    private readonly ConcurrentDictionary<Type, ConcurrentBag<IObserverRegistration>> _observersByType = new();

    private bool _isDisposed;
    
    public event EventHandler<NetworkExceptionEventArgs>? ObserverException;

    public NetworkObserverDispatcher()
    {
        var messageQueue = Channel.CreateUnbounded<ObserverMessage>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });

        _queueWriter = messageQueue.Writer;
        _queueReader = messageQueue.Reader;
        _cancellationTokenSource = new CancellationTokenSource();
        _dispatcherTask = DispatchLoopAsync(_cancellationTokenSource.Token);
    }

    public bool TryNotify(ProxyConnection connection, INetworkMessage message)
    {
        CheckIfDisposed();
        return _queueWriter.TryWrite(new ObserverMessage(connection, message));
    }

    public NetworkObserverRef AddObserver<T>(NetworkMessageObserver<T> observer, object? parameter = null) where T : class, INetworkMessage
    {
        CheckIfDisposed();
        
        var registration = new SyncObserverRegistration<T>(observer, parameter);
        var messageType = typeof(T);

        _observersByType.AddOrUpdate(
            messageType,
            [registration],
            (_, bag) =>
            {
                bag.Add(registration);
                return bag;
            });

        return new NetworkObserverRef(() => RemoveObserver(registration));
    }
    
    public NetworkObserverRef AddObserver<T>(AsyncNetworkMessageObserver<T> observer, object? parameter = null) where T : class, INetworkMessage
    {
        CheckIfDisposed();
        
        var registration = new AsyncObserverRegistration<T>(observer, parameter);
        var messageType = typeof(T);

        _observersByType.AddOrUpdate(
            messageType,
            [registration],
            (_, bag) =>
            {
                bag.Add(registration);
                return bag;
            });

        return new NetworkObserverRef(() => RemoveObserver(registration));
    }

    private static void RemoveObserver(IObserverRegistration registration)
    {
        registration.IsActive = false;
    }

    private async Task DispatchLoopAsync(CancellationToken token)
    {
        await foreach (var message in _queueReader.ReadAllAsync(token))
        {
            var messageType = message.Message.GetType();

            if (!_observersByType.TryGetValue(messageType, out var observers) || observers.IsEmpty)
            {
                continue;
            }

            // Rent an array to store active observers
            var count = 0;
            var hasInactiveObservers = false;
            var observerArray = ArrayPool<IObserverRegistration>.Shared.Rent(100);
            Task[]? taskArray = null;

            try
            {
                foreach (var observer in observers)
                {
                    if (!observer.IsActive)
                    {
                        hasInactiveObservers = true;
                        continue;
                    }

                    // If we've reached the array capacity, grow it
                    if (count >= observerArray.Length)
                    {
                        var oldArray = observerArray;
                        observerArray = ArrayPool<IObserverRegistration>.Shared.Rent(oldArray.Length * 2);
                        Array.Copy(oldArray, observerArray, oldArray.Length);
                        ArrayPool<IObserverRegistration>.Shared.Return(oldArray);
                    }

                    observerArray[count++] = observer;
                }

                // If we have any active observers, dispatch them to the message handler
                if (count > 0)
                {
                    taskArray = ArrayPool<Task>.Shared.Rent(count);
                    for (var i = 0; i < count; i++)
                    {
                        var observer = observerArray[i];
                        taskArray[i] = SafeHandleAsync(observer, message.Connection, message.Message);
                    }

                    await Task.WhenAll(taskArray);
                }
            }
            finally
            {
                ArrayPool<IObserverRegistration>.Shared.Return(observerArray);
                if (taskArray != null)
                {
                    ArrayPool<Task>.Shared.Return(taskArray);
                }
                
                if (hasInactiveObservers)
                {
                    CleanupInactiveObservers(messageType);
                }
            }
        }
    }

    private async Task SafeHandleAsync(IObserverRegistration observer, ProxyConnection connection,
        INetworkMessage message)
    {
        try
        {
            await observer.HandleAsync(connection, message);
        }
        catch (Exception ex)
        {
            ObserverException?.Invoke(observer, new NetworkExceptionEventArgs(ex));
        }
    }

    private void CleanupInactiveObservers(Type messageType)
    {
        if (!_observersByType.TryGetValue(messageType, out var observers))
        {
            return;
        }

        var activeObservers = observers.Where(o => o.IsActive).ToList();
        
        if (activeObservers.Count == 0)
        {
            // If there are no more active observers, remove the bag entirely
            _observersByType.TryRemove(messageType, out _);
        }
        else if (activeObservers.Count < observers.Count)
        {
            // Replace the bag with a new one containing only the active observers
            var newBag = new ConcurrentBag<IObserverRegistration>(activeObservers);
            _observersByType.AddOrUpdate(messageType, newBag, (_, _) => newBag);
        }
    }

    ~NetworkObserverDispatcher() => Dispose(false);

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
            _queueWriter.TryComplete();
            _cancellationTokenSource.Cancel();
            _dispatcherTask.Wait();
            _cancellationTokenSource.Dispose();
            _queueReader.Completion.Dispose();
        }

        _isDisposed = true;
    }

    private void CheckIfDisposed()
        => ObjectDisposedException.ThrowIf(_isDisposed, nameof(NetworkObserverDispatcher));
}