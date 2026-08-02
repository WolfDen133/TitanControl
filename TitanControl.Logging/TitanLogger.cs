using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Runtime.ExceptionServices;

namespace TitanControl.Logging;

public sealed class TitanLogger : IDisposable, IAsyncDisposable
{
    private abstract record LogCommand;

    private sealed record EntryCommand(LogEntry Entry) : LogCommand;

    private sealed record FlushCommand(
        TaskCompletionSource<bool>? Completion,
        bool Durable) : LogCommand;

    private sealed record OperationContext(
        Guid Id,
        Guid? ParentId,
        string Name,
        string? Category);

    private readonly LoggerOptions _options = null!;
    private readonly Channel<LogCommand> _commands = null!;
    private readonly CancellationTokenSource _flushTimerCancellation = new();
    private readonly Task _workerTask = null!;
    private readonly Task _flushTimerTask = null!;
    private readonly EmergencyLogWriter _emergencyWriter = null!;
    private readonly ConsoleWindowHost _console = new();
    private readonly ConcurrentQueue<LogEntry> _recentEntries = new();
    private readonly ConcurrentDictionary<Guid, OperationSnapshot> _activeOperations = new();
    private readonly AsyncLocal<OperationContext?> _currentOperation = new();
    private static readonly AsyncLocal<bool> FirstChanceHandlerActive = new();
    private readonly TaskCompletionSource<bool> _stopped =
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    private UnhandledExceptionEventHandler? _unhandledExceptionHandler;
    private EventHandler<UnobservedTaskExceptionEventArgs>? _unobservedTaskExceptionHandler;
    private EventHandler? _processExitHandler;
    private EventHandler<FirstChanceExceptionEventArgs>? _firstChanceExceptionHandler;

    private long _sequence;
    private int _recentEntryCount;
    private int _state; // 0 = running, 1 = stopping, 2 = stopped
    private int _consoleEnabled;
    private int _workerFailed;
    private string? _currentLogFilePath;

    private bool _designMode;

    public TitanLogger(LoggerOptions options, bool designMode = false)
    {
        if (designMode)
        {
            _designMode = designMode;
            return;
        }

        _options = options ?? throw new ArgumentNullException(nameof(options));
        _options.Validate();

        Directory.CreateDirectory(_options.LogDirectory);
        DeleteOldLogFilesBestEffort();

        _emergencyWriter = new EmergencyLogWriter(
            _options.LogDirectory,
            _options.FileNamePrefix);

        _commands = Channel.CreateUnbounded<LogCommand>(
            new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false,
                AllowSynchronousContinuations = false
            });

        _workerTask = Task.Run(ProcessCommandsAsync);
        _flushTimerTask = Task.Run(FlushTimerLoopAsync);

        if (_options.WriteToConsole)
        {
            _console.EnableExistingOutput();
            Volatile.Write(ref _consoleEnabled, 1);
        }

        if (_options.OpenConsoleOnStart)
            OpenConsole();

        if (_options.InstallGlobalExceptionHandlers)
            InstallGlobalExceptionHandlers();

        Information(
            "Logging initialized.",
            category: "Logging",
            properties: Properties(
                ("LogDirectory", _options.LogDirectory),
                ("FlushIntervalMs", _options.FlushInterval.TotalMilliseconds),
                ("MinimumLevel", _options.MinimumLevel)));
    }

    public string? CurrentLogFilePath => Volatile.Read(ref _currentLogFilePath);

    public bool IsRunning =>
        Volatile.Read(ref _state) == 0 &&
        Volatile.Read(ref _workerFailed) == 0;

    public IReadOnlyCollection<OperationSnapshot> GetActiveOperations() =>
        _activeOperations.Values
            .OrderBy(operation => operation.StartedAt)
            .ToArray();

    public void Trace(
        string message,
        string? category = null,
        IReadOnlyDictionary<string, object?>? properties = null) =>
        Write(LogLevel.Trace, message, null, category, properties);

    public void Debug(
        string message,
        string? category = null,
        IReadOnlyDictionary<string, object?>? properties = null) =>
        Write(LogLevel.Debug, message, null, category, properties);

    public void Information(
        string message,
        string? category = null,
        IReadOnlyDictionary<string, object?>? properties = null) =>
        Write(LogLevel.Information, message, null, category, properties);

    public void Warning(
        string message,
        string? category = null,
        IReadOnlyDictionary<string, object?>? properties = null) =>
        Write(LogLevel.Warning, message, null, category, properties);

    public void Warning(
        Exception exception,
        string message,
        string? category = null,
        IReadOnlyDictionary<string, object?>? properties = null) =>
        Write(LogLevel.Warning, message, exception, category, properties);

    public void Error(
        Exception exception,
        string message,
        string? category = null,
        IReadOnlyDictionary<string, object?>? properties = null) =>
        Write(LogLevel.Error, message, exception, category, properties);

    public void Error(
        string message,
        string? category = null,
        IReadOnlyDictionary<string, object?>? properties = null) =>
        Write(LogLevel.Error, message, null, category, properties);

    public void Critical(
        Exception exception,
        string message,
        string? category = null,
        IReadOnlyDictionary<string, object?>? properties = null) =>
        Write(LogLevel.Critical, message, exception, category, properties);

    public void Critical(
        string message,
        string? category = null,
        IReadOnlyDictionary<string, object?>? properties = null) =>
        Write(LogLevel.Critical, message, null, category, properties);

    public void Write(
        LogLevel level,
        string message,
        Exception? exception = null,
        string? category = null,
        IReadOnlyDictionary<string, object?>? properties = null)
    {
        if (_designMode) return;

        ArgumentNullException.ThrowIfNull(message);

        if (level < _options.MinimumLevel)
            return;

        LogEntry entry = CreateEntry(level, message, exception, category, properties);
        Enqueue(entry);
    }

    /// <summary>
    /// Opens the Windows console window and replays recent entries. On Linux and
    /// macOS it enables the existing stdout; it cannot create a new terminal window.
    /// </summary>
    public bool OpenConsole(bool replayRecentEntries = true)
    {
        if (!_console.Open($"{_options.FileNamePrefix} Logs"))
            return false;

        Volatile.Write(ref _consoleEnabled, 1);

        if (replayRecentEntries)
        {
            foreach (LogEntry entry in _recentEntries.ToArray())
                _console.Write(entry, LogFormatter.Format(entry));
        }

        return true;
    }

    public void CloseConsole()
    {
        Volatile.Write(ref _consoleEnabled, 0);
        _console.Close();
    }

    public async Task RunAsync(
        string operationName,
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken = default,
        string? category = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(operationName);
        ArgumentNullException.ThrowIfNull(operation);

        OperationContext? previous = _currentOperation.Value;
        OperationContext current = StartOperation(operationName, category, previous);
        var stopwatch = Stopwatch.StartNew();

        try
        {
            await operation(cancellationToken).ConfigureAwait(false);

            Debug(
                $"Operation completed: {operationName}",
                category,
                Properties(("DurationMs", stopwatch.Elapsed.TotalMilliseconds)));
        }
        catch (OperationCanceledException)
        {
            Information(
                $"Operation cancelled: {operationName}",
                category,
                Properties(
                    ("DurationMs", stopwatch.Elapsed.TotalMilliseconds),
                    ("CancellationTokenRequested", cancellationToken.IsCancellationRequested)));

            throw;
        }
        catch (Exception exception)
        {
            Error(
                exception,
                $"Operation failed: {operationName}",
                category,
                Properties(("DurationMs", stopwatch.Elapsed.TotalMilliseconds)));

            throw;
        }
        finally
        {
            FinishOperation(current, previous);
        }
    }

    public async Task<T> RunAsync<T>(
        string operationName,
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken = default,
        string? category = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(operationName);
        ArgumentNullException.ThrowIfNull(operation);

        OperationContext? previous = _currentOperation.Value;
        OperationContext current = StartOperation(operationName, category, previous);
        var stopwatch = Stopwatch.StartNew();

        try
        {
            T result = await operation(cancellationToken).ConfigureAwait(false);

            Debug(
                $"Operation completed: {operationName}",
                category,
                Properties(("DurationMs", stopwatch.Elapsed.TotalMilliseconds)));

            return result;
        }
        catch (OperationCanceledException)
        {
            Information(
                $"Operation cancelled: {operationName}",
                category,
                Properties(
                    ("DurationMs", stopwatch.Elapsed.TotalMilliseconds),
                    ("CancellationTokenRequested", cancellationToken.IsCancellationRequested)));

            throw;
        }
        catch (Exception exception)
        {
            Error(
                exception,
                $"Operation failed: {operationName}",
                category,
                Properties(("DurationMs", stopwatch.Elapsed.TotalMilliseconds)));

            throw;
        }
        finally
        {
            FinishOperation(current, previous);
        }
    }

    /// <summary>
    /// Starts an intentionally detached operation. Exceptions are observed and
    /// logged, then swallowed because there is no caller to await them.
    /// </summary>
    public void RunDetached(
        string operationName,
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken = default,
        string? category = null)
    {
        _ = RunDetachedCoreAsync(
            operationName,
            operation,
            cancellationToken,
            category);
    }

    public async Task FlushAsync(
        bool durable = false,
        CancellationToken cancellationToken = default)
    {
        if (Volatile.Read(ref _state) == 2)
            return;

        var completion = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously);

        if (!_commands.Writer.TryWrite(new FlushCommand(completion, durable)))
        {
            await _workerTask.WaitAsync(cancellationToken).ConfigureAwait(false);
            return;
        }

        await completion.Task.WaitAsync(cancellationToken).ConfigureAwait(false);
    }

    public bool FlushBlocking(bool durable = true, TimeSpan? timeout = null)
    {
        TimeSpan effectiveTimeout = timeout ?? _options.ShutdownTimeout;

        try
        {
            return FlushAsync(durable)
                .Wait(effectiveTimeout);
        }
        catch (Exception exception)
        {
            _emergencyWriter.WriteText(
                $"FlushBlocking failed: {exception}{Environment.NewLine}");
            return false;
        }
    }

    public void Dispose()
    {
        DisposeAsync().AsTask().GetAwaiter().GetResult();
    }

    public async ValueTask DisposeAsync()
    {
        int previousState = Interlocked.CompareExchange(ref _state, 1, 0);

        if (previousState != 0)
        {
            await _stopped.Task.ConfigureAwait(false);
            return;
        }

        try
        {
            // The public Enqueue path rejects new entries after state changes to
            // stopping, so place this final informational entry directly in the
            // channel before completing it.
            _commands.Writer.TryWrite(
                new EntryCommand(
                    CreateEntry(
                        LogLevel.Information,
                        "Logging shutdown requested.",
                        exception: null,
                        category: "Logging",
                        properties: null)));

            _flushTimerCancellation.Cancel();

            try
            {
                await _flushTimerTask.ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // Expected.
            }

            _commands.Writer.TryComplete();

            try
            {
                await _workerTask
                    .WaitAsync(_options.ShutdownTimeout)
                    .ConfigureAwait(false);
            }
            catch (TimeoutException)
            {
                _emergencyWriter.WriteText(
                    $"Logger shutdown exceeded {_options.ShutdownTimeout}.{Environment.NewLine}");
            }
        }
        finally
        {
            UninstallGlobalExceptionHandlers();
            _console.Close();
            _flushTimerCancellation.Dispose();

            Volatile.Write(ref _state, 2);
            _stopped.TrySetResult(true);
        }
    }

    private async Task RunDetachedCoreAsync(
        string operationName,
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken,
        string? category)
    {
        try
        {
            await RunAsync(
                operationName,
                operation,
                cancellationToken,
                category).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // RunAsync already logged cancellation.
        }
        catch
        {
            // RunAsync already logged the exception. Observing it here prevents an
            // intentionally detached task from becoming unobserved.
        }
    }

    private OperationContext StartOperation(
        string operationName,
        string? category,
        OperationContext? previous)
    {
        Guid id = Guid.NewGuid();
        var current = new OperationContext(
            id,
            previous?.Id,
            operationName,
            category);

        _currentOperation.Value = current;

        _activeOperations[id] = new OperationSnapshot(
            id,
            previous?.Id,
            operationName,
            category,
            GetTimestamp(),
            Environment.CurrentManagedThreadId);

        Debug(
            $"Operation started: {operationName}",
            category);

        return current;
    }

    private void FinishOperation(
        OperationContext current,
        OperationContext? previous)
    {
        _activeOperations.TryRemove(current.Id, out _);
        _currentOperation.Value = previous;
    }

    private void Enqueue(LogEntry entry)
    {
        if (Volatile.Read(ref _state) != 0 ||
            Volatile.Read(ref _workerFailed) != 0)
        {
            if (entry.Level >= LogLevel.Error)
                _emergencyWriter.Write(entry, "Logger was stopping or stopped.");

            return;
        }

        if (!_commands.Writer.TryWrite(new EntryCommand(entry)) &&
            entry.Level >= LogLevel.Error)
        {
            _emergencyWriter.Write(entry, "The logging queue was unavailable.");
        }
    }

    private LogEntry CreateEntry(
        LogLevel level,
        string message,
        Exception? exception,
        string? category,
        IReadOnlyDictionary<string, object?>? properties)
    {
        OperationContext? operation = _currentOperation.Value;

        return new LogEntry(
            Interlocked.Increment(ref _sequence),
            GetTimestamp(),
            level,
            message,
            category,
            exception,
            Environment.CurrentManagedThreadId,
            operation?.Id,
            operation?.ParentId,
            CopyProperties(properties));
    }

    private DateTimeOffset GetTimestamp() =>
        _options.UseUtcTimestamps
            ? DateTimeOffset.UtcNow
            : DateTimeOffset.Now;

    private async Task FlushTimerLoopAsync()
    {
        try
        {
            using var timer = new PeriodicTimer(_options.FlushInterval);

            while (await timer.WaitForNextTickAsync(
                       _flushTimerCancellation.Token).ConfigureAwait(false))
            {
                if (!_commands.Writer.TryWrite(
                        new FlushCommand(Completion: null, Durable: false)))
                {
                    return;
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Normal logger shutdown.
        }
        catch (Exception exception)
        {
            _emergencyWriter.WriteText(
                $"The logging flush timer failed: {exception}{Environment.NewLine}");
        }
    }

    private async Task ProcessCommandsAsync()
    {
        StreamWriter? writer = null;
        FileStream? fileStream = null;
        DateOnly? openDate = null;
        Exception? workerFailure = null;

        try
        {
            await foreach (LogCommand command in _commands.Reader.ReadAllAsync())
            {
                switch (command)
                {
                    case EntryCommand entryCommand:
                    {
                        DateOnly entryDate = GetDate(entryCommand.Entry.Timestamp);

                        if (writer is null || openDate != entryDate)
                        {
                            await CloseWriterAsync(
                                writer,
                                fileStream,
                                durable: true).ConfigureAwait(false);

                            (writer, fileStream) = await OpenWriterAsync(
                                entryDate).ConfigureAwait(false);

                            openDate = entryDate;
                        }

                        string formatted = LogFormatter.Format(entryCommand.Entry);
                        await writer.WriteLineAsync(formatted).ConfigureAwait(false);

                        Remember(entryCommand.Entry);

                        if (Volatile.Read(ref _consoleEnabled) != 0)
                            _console.Write(entryCommand.Entry, formatted);

                        if (entryCommand.Entry.Level >= _options.ImmediateFlushLevel)
                        {
                            bool durable =
                                entryCommand.Entry.Level == LogLevel.Critical &&
                                _options.DurableFlushOnCritical;

                            await FlushWriterAsync(
                                writer,
                                fileStream,
                                durable).ConfigureAwait(false);
                        }

                        break;
                    }

                    case FlushCommand flushCommand:
                    {
                        try
                        {
                            await FlushWriterAsync(
                                writer,
                                fileStream,
                                flushCommand.Durable).ConfigureAwait(false);

                            flushCommand.Completion?.TrySetResult(true);
                        }
                        catch (Exception exception)
                        {
                            flushCommand.Completion?.TrySetException(exception);
                            throw;
                        }

                        break;
                    }
                }
            }
        }
        catch (Exception exception)
        {
            workerFailure = exception;
            Volatile.Write(ref _workerFailed, 1);
            _commands.Writer.TryComplete(exception);
            _emergencyWriter.WriteText(
                $"The main logging worker failed: {exception}{Environment.NewLine}");
        }
        finally
        {
            try
            {
                await CloseWriterAsync(
                    writer,
                    fileStream,
                    durable: true).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                _emergencyWriter.WriteText(
                    $"Final log-file close failed: {exception}{Environment.NewLine}");
            }

            if (workerFailure is not null)
            {
                while (_commands.Reader.TryRead(out LogCommand? command))
                {
                    if (command is FlushCommand flushCommand)
                        flushCommand.Completion?.TrySetException(workerFailure);
                }
            }
        }
    }

    private async Task<(StreamWriter Writer, FileStream Stream)> OpenWriterAsync(
        DateOnly date)
    {
        Directory.CreateDirectory(_options.LogDirectory);

        string path = Path.Combine(
            _options.LogDirectory,
            $"{_options.FileNamePrefix}-{date:yyyyMMdd}-p{Environment.ProcessId}.log");

        var stream = new FileStream(
            path,
            new FileStreamOptions
            {
                Mode = FileMode.Append,
                Access = FileAccess.Write,
                Share = FileShare.ReadWrite | FileShare.Delete,
                BufferSize = 16 * 1024,
                Options = FileOptions.Asynchronous | FileOptions.SequentialScan
            });

        var writer = new StreamWriter(
            stream,
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
            bufferSize: 16 * 1024,
            leaveOpen: true)
        {
            AutoFlush = false
        };

        Volatile.Write(ref _currentLogFilePath, path);

        if (stream.Length == 0)
        {
            await writer.WriteLineAsync(
                $"# {_options.FileNamePrefix} log; process={Environment.ProcessId}; " +
                $"started={GetTimestamp():O}").ConfigureAwait(false);
        }

        return (writer, stream);
    }

    private static async Task FlushWriterAsync(
        StreamWriter? writer,
        FileStream? fileStream,
        bool durable)
    {
        if (writer is null)
            return;

        await writer.FlushAsync().ConfigureAwait(false);

        if (durable)
            fileStream?.Flush(flushToDisk: true);
    }

    private static async Task CloseWriterAsync(
        StreamWriter? writer,
        FileStream? fileStream,
        bool durable)
    {
        if (writer is null)
            return;

        await FlushWriterAsync(writer, fileStream, durable).ConfigureAwait(false);
        await writer.DisposeAsync().ConfigureAwait(false);
        fileStream?.Dispose();
    }

    private void Remember(LogEntry entry)
    {
        if (_options.RecentEntryCapacity == 0)
            return;

        _recentEntries.Enqueue(entry);
        int count = Interlocked.Increment(ref _recentEntryCount);

        while (count > _options.RecentEntryCapacity &&
               _recentEntries.TryDequeue(out _))
        {
            count = Interlocked.Decrement(ref _recentEntryCount);
        }
    }

    private void InstallGlobalExceptionHandlers()
    {
        _unhandledExceptionHandler = (_, eventArgs) =>
        {
            Exception exception = eventArgs.ExceptionObject as Exception
                ?? new Exception(
                    $"Unhandled non-Exception object: " +
                    $"{eventArgs.ExceptionObject}");

            LastChanceCritical(
                exception,
                $"Unhandled AppDomain exception. " +
                $"IsTerminating={eventArgs.IsTerminating}",
                "AppDomain");
        };

        _unobservedTaskExceptionHandler = (_, eventArgs) =>
        {
            Error(
                eventArgs.Exception,
                "Unobserved task exception.",
                "TaskScheduler");

            if (_options.ObserveUnobservedTaskExceptions)
                eventArgs.SetObserved();
        };

        _processExitHandler = (_, _) =>
        {
            FlushBlocking(
                durable: true,
                timeout: _options.ShutdownTimeout);
        };

        if (_options.CaptureFirstChanceExceptions)
        {
            _firstChanceExceptionHandler = (_, eventArgs) =>
            {
                if (FirstChanceHandlerActive.Value)
                    return;

                try
                {
                    FirstChanceHandlerActive.Value = true;

                    Exception exception = eventArgs.Exception;

                    if (_options.FirstChanceExceptionFilter is not null &&
                        !_options.FirstChanceExceptionFilter(exception))
                    {
                        return;
                    }

                    Warning(
                        exception,
                        "First-chance exception was thrown.",
                        "FirstChanceException");
                }
                catch
                {
                    // Never allow exception diagnostics to crash the application.
                }
                finally
                {
                    FirstChanceHandlerActive.Value = false;
                }
            };

            AppDomain.CurrentDomain.FirstChanceException +=
                _firstChanceExceptionHandler;
        }

        AppDomain.CurrentDomain.UnhandledException +=
            _unhandledExceptionHandler;

        TaskScheduler.UnobservedTaskException +=
            _unobservedTaskExceptionHandler;

        AppDomain.CurrentDomain.ProcessExit +=
            _processExitHandler;
    }

    private void UninstallGlobalExceptionHandlers()
    {
        if (_firstChanceExceptionHandler is not null)
        {
            AppDomain.CurrentDomain.FirstChanceException -=
                _firstChanceExceptionHandler;
        }

        if (_unhandledExceptionHandler is not null)
        {
            AppDomain.CurrentDomain.UnhandledException -=
                _unhandledExceptionHandler;
        }

        if (_unobservedTaskExceptionHandler is not null)
        {
            TaskScheduler.UnobservedTaskException -=
                _unobservedTaskExceptionHandler;
        }

        if (_processExitHandler is not null)
        {
            AppDomain.CurrentDomain.ProcessExit -=
                _processExitHandler;
        }

        _firstChanceExceptionHandler = null;
        _unhandledExceptionHandler = null;
        _unobservedTaskExceptionHandler = null;
        _processExitHandler = null;
    }

    private void LastChanceCritical(
        Exception exception,
        string message,
        string category)
    {
        LogEntry entry = CreateEntry(
            LogLevel.Critical,
            message,
            exception,
            category,
            properties: null);

        // Write a separate emergency copy before waiting on the background worker.
        // This path intentionally uses only a private, short-lived lock.
        _emergencyWriter.Write(entry, "Last-chance exception handler.");

        Enqueue(entry);

        FlushBlocking(
            durable: true,
            timeout: _options.ShutdownTimeout);
    }

    private void DeleteOldLogFilesBestEffort()
    {
        if (_options.RetainedFileCount == 0)
            return;

        try
        {
            Directory.CreateDirectory(_options.LogDirectory);

            DateTime preserveAfter = DateTime.UtcNow.AddDays(-1);

            FileInfo[] candidates = new DirectoryInfo(_options.LogDirectory)
                .EnumerateFiles($"{_options.FileNamePrefix}-*-p*.log")
                .Where(file => file.LastWriteTimeUtc < preserveAfter)
                .OrderByDescending(file => file.LastWriteTimeUtc)
                .ToArray();

            foreach (FileInfo file in candidates.Skip(_options.RetainedFileCount))
            {
                try
                {
                    file.Delete();
                }
                catch
                {
                    // Retention cleanup is non-critical.
                }
            }
        }
        catch
        {
            // Logging must still start even when cleanup fails.
        }
    }

    private static IReadOnlyDictionary<string, object?>? CopyProperties(
        IReadOnlyDictionary<string, object?>? properties)
    {
        if (properties is null || properties.Count == 0)
            return null;

        var copy = new Dictionary<string, object?>(
            properties.Count,
            StringComparer.Ordinal);

        foreach (KeyValuePair<string, object?> pair in properties)
            copy[pair.Key] = pair.Value;

        return copy;
    }

    private static IReadOnlyDictionary<string, object?> Properties(
        params (string Name, object? Value)[] values)
    {
        var properties = new Dictionary<string, object?>(
            values.Length,
            StringComparer.Ordinal);

        foreach ((string name, object? value) in values)
            properties[name] = value;

        return properties;
    }

    private DateOnly GetDate(DateTimeOffset timestamp) =>
        DateOnly.FromDateTime(
            _options.UseUtcTimestamps
                ? timestamp.UtcDateTime
                : timestamp.LocalDateTime);
}
