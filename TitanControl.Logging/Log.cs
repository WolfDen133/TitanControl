using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TitanControl.Logging;

/// <summary>
/// Application-wide convenience facade. Initialize this at the first line of
/// Program.Main, before creating Avalonia controls or starting background tasks.
/// </summary>
public static class Log
{
    private static readonly object Sync = new();
    private static TitanLogger? _current;

    public static bool IsInitialized
    {
        get
        {
            lock (Sync)
                return _current is not null;
        }
    }

    public static TitanLogger Current
    {
        get
        {
            lock (Sync)
            {
                return _current
                    ?? throw new InvalidOperationException(
                        "Logging is not initialized. Call Log.Initialize(...) first.");
            }
        }
    }

    public static TitanLogger Initialize(LoggerOptions? options = null)
    {
        lock (Sync)
        {
            if (_current is not null)
                throw new InvalidOperationException("Logging is already initialized.");

            _current = new TitanLogger(options ?? new LoggerOptions());
            return _current;
        }
    }

    public static void InitializeDesign()
    {
        lock (Sync)
        {
            _current = new TitanLogger(new LoggerOptions(), true);
        }
    }

    public static void Trace(
        string message,
        string? category = null,
        IReadOnlyDictionary<string, object?>? properties = null) =>
        Current.Trace(message, category, properties);

    public static void Debug(
        string message,
        string? category = null,
        IReadOnlyDictionary<string, object?>? properties = null) =>
        Current.Debug(message, category, properties);

    public static void Information(
        string message,
        string? category = null,
        IReadOnlyDictionary<string, object?>? properties = null) =>
        Current.Information(message, category, properties);

    public static void Warning(
        string message,
        string? category = null,
        IReadOnlyDictionary<string, object?>? properties = null) =>
        Current.Warning(message, category, properties);

    public static void Warning(
        Exception exception,
        string message,
        string? category = null,
        IReadOnlyDictionary<string, object?>? properties = null) =>
        Current.Warning(exception, message, category, properties);

    public static void Error(
        Exception exception,
        string message,
        string? category = null,
        IReadOnlyDictionary<string, object?>? properties = null) =>
        Current.Error(exception, message, category, properties);

    public static void Error(
        string message,
        string? category = null,
        IReadOnlyDictionary<string, object?>? properties = null) =>
        Current.Error(message, category, properties);

    public static void Critical(
        Exception exception,
        string message,
        string? category = null,
        IReadOnlyDictionary<string, object?>? properties = null) =>
        Current.Critical(exception, message, category, properties);

    public static void Critical(
        string message,
        string? category = null,
        IReadOnlyDictionary<string, object?>? properties = null) =>
        Current.Critical(message, category, properties);

    public static Task RunAsync(
        string operationName,
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken = default,
        string? category = null) =>
        Current.RunAsync(
            operationName,
            operation,
            cancellationToken,
            category);

    public static Task<T> RunAsync<T>(
        string operationName,
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken = default,
        string? category = null) =>
        Current.RunAsync(
            operationName,
            operation,
            cancellationToken,
            category);

    public static void RunDetached(
        string operationName,
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken = default,
        string? category = null) =>
        Current.RunDetached(
            operationName,
            operation,
            cancellationToken,
            category);

    public static IReadOnlyCollection<OperationSnapshot> GetActiveOperations() =>
        Current.GetActiveOperations();

    public static bool OpenConsole(bool replayRecentEntries = true) =>
        Current.OpenConsole(replayRecentEntries);

    public static void CloseConsole() => Current.CloseConsole();

    public static Task FlushAsync(
        bool durable = false,
        CancellationToken cancellationToken = default) =>
        Current.FlushAsync(durable, cancellationToken);

    public static bool FlushBlocking(
        bool durable = true,
        TimeSpan? timeout = null) =>
        Current.FlushBlocking(durable, timeout);

    public static void Shutdown()
    {
        TitanLogger? logger;

        lock (Sync)
            logger = _current;

        logger?.Dispose();

        lock (Sync)
        {
            if (ReferenceEquals(_current, logger))
                _current = null;
        }
    }

    public static async ValueTask ShutdownAsync()
    {
        TitanLogger? logger;

        lock (Sync)
            logger = _current;

        if (logger is not null)
            await logger.DisposeAsync().ConfigureAwait(false);

        lock (Sync)
        {
            if (ReferenceEquals(_current, logger))
                _current = null;
        }
    }
}
