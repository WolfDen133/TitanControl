using System;

namespace TitanControl.Logging;

public sealed class LoggerOptions
{
    public LogLevel MinimumLevel { get; init; } = LogLevel.Debug;

    public string LogDirectory { get; init; } = GetDefaultLogDirectory();

    public string FileNamePrefix { get; init; } = "TitanControl";

    /// <summary>
    /// Buffered file output is flushed at approximately this interval.
    /// Error and Critical entries can be flushed sooner.
    /// </summary>
    public TimeSpan FlushInterval { get; init; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Maximum time used by synchronous shutdown and last-chance flushes.
    /// </summary>
    public TimeSpan ShutdownTimeout { get; init; } = TimeSpan.FromSeconds(4);

    /// <summary>
    /// Entries at or above this level trigger an immediate file flush.
    /// </summary>
    public LogLevel ImmediateFlushLevel { get; init; } = LogLevel.Error;

    /// <summary>
    /// For Critical entries, ask the operating system to flush the file stream
    /// through to the storage device after the normal StreamWriter flush.
    /// </summary>
    public bool DurableFlushOnCritical { get; init; } = true;

    public bool InstallGlobalExceptionHandlers { get; init; } = true;

    public bool ObserveUnobservedTaskExceptions { get; init; } = true;

    /// <summary>
    /// Write new entries to an already attached console/stdout.
    /// This does not create a new terminal window.
    /// </summary>
    public bool WriteToConsole { get; init; }

    /// <summary>
    /// On Windows, allocate a console window during logger startup.
    /// On other platforms, the logger can only use an existing terminal/stdout.
    /// </summary>
    public bool OpenConsoleOnStart { get; init; }

    /// <summary>
    /// Number of already-written entries kept in memory so they can be replayed
    /// when the console is opened later.
    /// </summary>
    public int RecentEntryCapacity { get; init; } = 500;

    /// <summary>
    /// Maximum number of old process log files retained. Files modified during
    /// the last day are never removed by this cleanup.
    /// Set to 0 to disable cleanup.
    /// </summary>
    public int RetainedFileCount { get; init; } = 30;

    /// <summary>
    /// Records exceptions when they are first thrown, even when they are later
    /// caught. Intended for temporary diagnostics only.
    /// </summary>
    public bool CaptureFirstChanceExceptions { get; init; }

    /// <summary>
    /// Optional filter for first-chance exception capture.
    /// Return true to record the exception.
    /// </summary>
    public Func<Exception, bool>? FirstChanceExceptionFilter { get; init; }

    public bool UseUtcTimestamps { get; init; }

    internal void Validate()
    {
        if (string.IsNullOrWhiteSpace(LogDirectory))
            throw new ArgumentException("A log directory is required.", nameof(LogDirectory));

        if (string.IsNullOrWhiteSpace(FileNamePrefix))
            throw new ArgumentException("A file-name prefix is required.", nameof(FileNamePrefix));

        if (FileNamePrefix.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) >= 0)
        {
            throw new ArgumentException(
                "FileNamePrefix contains characters that are invalid in a file name.",
                nameof(FileNamePrefix));
        }

        if (FlushInterval <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(FlushInterval), "FlushInterval must be positive.");

        if (ShutdownTimeout <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(ShutdownTimeout), "ShutdownTimeout must be positive.");

        if (RecentEntryCapacity < 0)
            throw new ArgumentOutOfRangeException(nameof(RecentEntryCapacity));

        if (RetainedFileCount < 0)
            throw new ArgumentOutOfRangeException(nameof(RetainedFileCount));
    }

    public static string GetDefaultLogDirectory()
    {
        string root = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        if (string.IsNullOrWhiteSpace(root))
            root = AppContext.BaseDirectory;

        return System.IO.Path.Combine(root, "TitanControl", "Logs");
    }
}
