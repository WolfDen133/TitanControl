using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using TitanControl.Logging;
using TitanControl.WebAPI.Queue.Interface;

namespace TitanControl.WebAPI.Queue
{
    public sealed class PriorityTaskQueue : IDisposable, IAsyncDisposable
    {
        private readonly object _queueLock = new();

        private readonly PriorityQueue<IWorkItem, int> _queue = new();

        private readonly SemaphoreSlim _signal = new(0);
        private readonly CancellationTokenSource _shutdownCts = new();

        private Task? _worker;

        private int _started;
        private int _disposed;

        public bool IsRunning =>
            Volatile.Read(ref _started) == 1 &&
            !_shutdownCts.IsCancellationRequested;

        public int PendingCount
        {
            get
            {
                lock (_queueLock)
                {
                    return _queue.Count;
                }
            }
        }

        public void Start()
        {
            ThrowIfDisposed();

            if (Interlocked.CompareExchange(ref _started, 1, 0) != 0)
                return;

            Log.Information(
                "Priority task queue started.",
                category: "TaskQueue");

            _worker = Task.Run(ProcessQueueAsync);
        }

        public Task<TResult> Enqueue<TResult>(
            Func<CancellationToken, Task<TResult>> work,
            int priority = 2,
            string? operationName = null,
            string category = "TaskQueue",
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(work);
            ThrowIfDisposed();

            if (Volatile.Read(ref _started) == 0)
            {
                throw new InvalidOperationException(
                    "The priority task queue must be started before work can be enqueued.");
            }

            if (_shutdownCts.IsCancellationRequested)
            {
                return Task.FromCanceled<TResult>(
                    new CancellationToken(canceled: true));
            }

            if (cancellationToken.IsCancellationRequested)
                return Task.FromCanceled<TResult>(cancellationToken);

            operationName ??= work.Method.Name;

            var item = new WorkItem<TResult>(
                work,
                operationName,
                category,
                priority,
                cancellationToken);

            lock (_queueLock)
            {
                if (_shutdownCts.IsCancellationRequested)
                {
                    item.Cancel(_shutdownCts.Token);
                    return item.Task;
                }

                _queue.Enqueue(item, priority);
            }

            Log.Trace(
                $"Queued operation '{operationName}' with priority {priority}.",
                category);

            _signal.Release();

            return item.Task;
        }

        public Task<TResult> Enqueue<TResult>(
            Func<CancellationToken, Task<TResult>> work,
            TaskPriority priority,
            string? operationName = null,
            string category = "TaskQueue",
            CancellationToken cancellationToken = default)
        {
            return Enqueue(
                work,
                (int)priority,
                operationName,
                category,
                cancellationToken);
        }

        public Task Enqueue(
            Func<CancellationToken, Task> work,
            int priority = 2,
            string? operationName = null,
            string category = "TaskQueue",
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(work);

            return Enqueue(
                async token =>
                {
                    await work(token).ConfigureAwait(false);
                    return true;
                },
                priority,
                operationName,
                category,
                cancellationToken);
        }

        public Task Enqueue(
            Func<CancellationToken, Task> work,
            TaskPriority priority,
            string? operationName = null,
            string category = "TaskQueue",
            CancellationToken cancellationToken = default)
        {
            return Enqueue(
                work,
                (int)priority,
                operationName,
                category,
                cancellationToken);
        }

        private async Task ProcessQueueAsync()
        {
            Log.Trace(
                "Priority task queue worker started.",
                category: "TaskQueue");

            try
            {
                while (true)
                {
                    await _signal
                        .WaitAsync(_shutdownCts.Token)
                        .ConfigureAwait(false);

                    if (_shutdownCts.IsCancellationRequested)
                        break;

                    IWorkItem? item;

                    lock (_queueLock)
                    {
                        item = _queue.Count > 0
                            ? _queue.Dequeue()
                            : null;
                    }

                    if (item is null)
                        continue;

                    await ExecuteItemSafelyAsync(item)
                        .ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
                when (_shutdownCts.IsCancellationRequested)
            {
                Log.Warning(
                    "Priority task queue worker was canceled during shutdown.",
                    category: "TaskQueue");
            }
            catch (Exception exception)
            {
                /*
                 * This indicates a failure in the queue infrastructure itself,
                 * rather than a normal exception from a queued operation.
                 */
                Log.Critical(
                    exception,
                    "The priority task queue worker terminated unexpectedly.",
                    category: "TaskQueue");

                _shutdownCts.Cancel();
            }
            finally
            {
                CancelPendingItems();

                Log.Information(
                    "Priority task queue worker stopped.",
                    category: "TaskQueue");
            }
        }

        private async Task ExecuteItemSafelyAsync(IWorkItem item)
        {
            try
            {
                await item.ExecuteAsync(_shutdownCts.Token)
                    .ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                /*
                 * WorkItem.ExecuteAsync is expected to catch operation errors.
                 * Reaching this catch indicates an error in the queue or WorkItem
                 * implementation itself.
                 */
                Log.Error(
                    exception,
                    $"The queue failed while processing operation " +
                    $"'{item.OperationName}'.",
                    category: "TaskQueue");

                item.Fail(exception);
            }
        }

        public async Task StopAsync()
        {
            if (Interlocked.Exchange(ref _started, 0) == 0)
                return;

            var sw = Stopwatch.StartNew();

            Log.Information(
                "Stopping priority task queue.",
                "TaskQueue");

            _shutdownCts.Cancel();

            try
            {
                _signal.Release();
            }
            catch (ObjectDisposedException)
            {
                return;
            }

            Task? worker = _worker;

            if (worker is not null)
            {

                try
                {
                    await worker.ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                    when (_shutdownCts.IsCancellationRequested)
                {
                }
            }


            CancelPendingItems();

            await Log.FlushAsync().ConfigureAwait(false);;
        }

        public void Stop()
        {
            StopAsync().GetAwaiter().GetResult();
        }

        private void CancelPendingItems()
        {
            List<IWorkItem> remainingItems = [];

            lock (_queueLock)
            {
                while (_queue.Count > 0)
                    remainingItems.Add(_queue.Dequeue());
            }

            foreach (IWorkItem item in remainingItems)
                item.Cancel(_shutdownCts.Token);

            if (remainingItems.Count > 0)
            {
                Log.Warning(
                    $"Canceled {remainingItems.Count} queued operation(s) " +
                    "during shutdown.",
                    category: "TaskQueue");
            }
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
                return;

            try
            {
                Stop();
            }
            finally
            {
                _shutdownCts.Dispose();
                _signal.Dispose();
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
                return;

            try
            {
                await StopAsync().ConfigureAwait(false);
            }
            finally
            {
                _shutdownCts.Dispose();
                _signal.Dispose();
            }

            GC.SuppressFinalize(this);
        }

        private void ThrowIfDisposed()
        {
            ObjectDisposedException.ThrowIf(
                Volatile.Read(ref _disposed) != 0,
                this);
        }
    }

}
