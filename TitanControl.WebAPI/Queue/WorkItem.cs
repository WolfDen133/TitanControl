using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanControl.WebAPI.Queue.Interface;
using TitanControl.Logging;

namespace TitanControl.WebAPI.Queue
{
    public sealed class WorkItem<TResult> : IWorkItem
    {
        private readonly Func<CancellationToken, Task<TResult>> _work;
        private readonly CancellationToken _callerCancellationToken;

        private readonly TaskCompletionSource<TResult> _completionSource =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        private readonly string _category;
        private readonly int _priority;

        public WorkItem(
            Func<CancellationToken, Task<TResult>> work,
            string operationName,
            string category,
            int priority,
            CancellationToken callerCancellationToken)
        {
            _work = work;
            OperationName = operationName;
            _category = category;
            _priority = priority;
            _callerCancellationToken = callerCancellationToken;
        }

        public string OperationName { get; }

        public Task<TResult> Task => _completionSource.Task;

        public async Task ExecuteAsync(
            CancellationToken queueCancellationToken)
        {
            using var linkedCts =
                CancellationTokenSource.CreateLinkedTokenSource(
                    queueCancellationToken,
                    _callerCancellationToken);

            CancellationToken token = linkedCts.Token;

            if (token.IsCancellationRequested)
            {
                _completionSource.TrySetCanceled(token);
                return;
            }

            try
            {
                /*
                 * Log.RunAsync provides:
                 *
                 * - Operation correlation.
                 * - Start and completion logging.
                 * - Duration measurement.
                 * - Exception logging.
                 * - Active-operation tracking.
                 *
                 * It rethrows failures, allowing this WorkItem to pass the
                 * same exception back to the caller.
                 */
                TResult result = await Log.RunAsync(
                    OperationName,
                    _work,
                    token,
                    _category).ConfigureAwait(false);

                _completionSource.TrySetResult(result);
            }
            catch (OperationCanceledException)
                when (token.IsCancellationRequested)
            {
                Log.Warning(
                    $"Queued operation '{OperationName}' was canceled.",
                    _category);

                _completionSource.TrySetCanceled(token);
            }
            catch (Exception exception)
            {
                /*
                 * Log.RunAsync already logs this exception. Do not call
                 * Log.Error here as well unless duplicate entries are wanted.
                 */
                _completionSource.TrySetException(exception);
            }
        }

        public void Cancel(CancellationToken cancellationToken)
        {
            Log.Warning(
                $"Queued operation '{OperationName}' was canceled " +
                "before execution.",
                _category);

            _completionSource.TrySetCanceled(cancellationToken);
        }

        public void Fail(Exception exception)
        {
            _completionSource.TrySetException(exception);
        }
    }
}
