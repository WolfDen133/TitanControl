using System;
using System.Threading;
using Avalonia.Threading;

namespace TitanControl.Logging;

/// <summary>
/// Optional Avalonia integration. This file requires a reference to Avalonia.Base.
/// Remove it if the logging project is intended to have no Avalonia dependency.
/// </summary>
public static class AvaloniaLogging
{
    public static IDisposable InstallDispatcherExceptionLogging(
        bool markExceptionsHandled = false)
    {
        var logger = Log.Current;

        DispatcherUnhandledExceptionEventHandler handler = (_, eventArgs) =>
        {
            try
            {
                logger.Error(
                    eventArgs.Exception,
                    "Unhandled Avalonia UI dispatcher exception.",
                    "Avalonia.Dispatcher");
            }
            catch
            {
                // An exception handler must not throw a second exception.
            }

            if (markExceptionsHandled)
                eventArgs.Handled = true;
        };

        Dispatcher.UIThread.UnhandledException += handler;

        return new CallbackDisposable(
            () => Dispatcher.UIThread.UnhandledException -= handler);
    }

    private sealed class CallbackDisposable : IDisposable
    {
        private Action? _callback;

        public CallbackDisposable(Action callback)
        {
            _callback = callback;
        }

        public void Dispose()
        {
            Interlocked.Exchange(ref _callback, null)?.Invoke();
        }
    }
}
