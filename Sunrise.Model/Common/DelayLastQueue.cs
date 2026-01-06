using System.Collections.Concurrent;

namespace Sunrise.Model.Common;

public sealed class DelayLastQueue
{
    private readonly object _syncLock = new();
    private Task? _task;
    private readonly ConcurrentQueue<Action> _queue = new();
    private readonly TimeSpan _delay;

    public DelayLastQueue(TimeSpan delay)
        => _delay = delay;

    public void Add(Action action)
    {
        _queue.Enqueue(action ?? throw new ArgumentNullException(nameof(action)));

        lock (_syncLock)
            _task ??= Task.Factory.StartNew(Run);
    }

    public async ValueTask WaitAsync()
    {
        Task task;

        lock (_syncLock)
            task = _task;

        if (task is not null && task.Status != TaskStatus.RanToCompletion)
            await task.ConfigureAwait(false);
    }

    private async void Run()
    {
        while (true)
        {
            try
            {
                Action lastAction = null;

                while (_queue.TryDequeue(out var action))
                    lastAction = action;

                lastAction?.Invoke();
                await Task.Delay(_delay).ConfigureAwait(false);

                lock (_syncLock)
                {
                    if (_queue.IsEmpty)
                    {
                        _task = null;
                        break;
                    }
                }
            }
            catch
            {
                lock (_syncLock)
                    _task = null;
            }
        }
    }

}
