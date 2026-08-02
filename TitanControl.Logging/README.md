# TitanControl.Logging

A dependency-light custom logger for a .NET 8+ Avalonia application.

## Features

- Six levels: Trace, Debug, Information, Warning, Error, Critical
- Multi-producer, single-consumer asynchronous queue
- One serialized file writer
- Five-second periodic flush by default
- Immediate flush for Error and Critical
- Durable `FileStream.Flush(true)` for Critical by default
- Daily/process-separated files
- Windows console allocation plus recent-entry replay
- Global AppDomain, ProcessExit, and unobserved Task hooks
- Async operation correlation with `AsyncLocal`
- Active operation map backed by `ConcurrentDictionary`
- `RunAsync` and `RunDetached` wrappers that log operation failures

## Add to your project

Copy the `.cs` files into a `Logging` folder in the TitanControl project.

`AvaloniaLogging.cs` requires Avalonia. The rest of the logger only uses .NET APIs.

Target framework recommendation:

```xml
<TargetFramework>net8.0</TargetFramework>
```

## Startup

Initialize logging before constructing Avalonia:

```csharp
using TitanControl.Logging;

public static class Program
{
    public static void Main(string[] args)
    {
        Log.Initialize(new LoggerOptions
        {
            MinimumLevel = LogLevel.Debug,
            FlushInterval = TimeSpan.FromSeconds(5),
            ImmediateFlushLevel = LogLevel.Error,
            DurableFlushOnCritical = true,
            OpenConsoleOnStart = false,
            WriteToConsole = false
        });

        try
        {
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }
        catch (Exception exception)
        {
            Log.Critical(
                exception,
                "TitanControl terminated unexpectedly.",
                "Program");

            Environment.ExitCode = 1;
        }
        finally
        {
            Log.Shutdown();
        }
    }
}
```

The explicit `finally` shutdown is the primary clean-exit path. The ProcessExit hook is a last-chance fallback.

## Avalonia UI exception hook

Install this after the Avalonia framework has initialized:

```csharp
private IDisposable? _dispatcherLogging;

public override void OnFrameworkInitializationCompleted()
{
    _dispatcherLogging =
        AvaloniaLogging.InstallDispatcherExceptionLogging(
            markExceptionsHandled: false);

    base.OnFrameworkInitializationCompleted();
}
```

Leaving `markExceptionsHandled` as `false` is safer. Marking an unknown UI exception as handled can allow the program to continue in an inconsistent state.

## Normal logging

```csharp
Log.Information("Application started.", "Startup");

Log.Warning(
    "The session is taking longer than expected.",
    "Session");

try
{
    await SaveAsync();
}
catch (Exception exception)
{
    Log.Error(
        exception,
        "Saving the project failed.",
        "Project");
}
```

Properties are copied before being queued, so later mutations to the original dictionary do not race with the writer:

```csharp
Log.Information(
    "Connected to session.",
    "Session",
    new Dictionary<string, object?>
    {
        ["SessionId"] = session.Id,
        ["Host"] = session.Host,
        ["LatencyMs"] = latency.TotalMilliseconds
    });
```

## Tracked async operations

```csharp
await Log.RunAsync(
    "Connect to session",
    async cancellationToken =>
    {
        await session.ConnectAsync(cancellationToken);
        await session.SynchronizeAsync(cancellationToken);
    },
    cancellationToken,
    category: "Session");
```

Every entry written inside the operation receives the same operation ID. Nested `RunAsync` calls also include the parent operation ID.

For an operation with a result:

```csharp
Project project = await Log.RunAsync(
    "Load project",
    cancellationToken => projectStore.LoadAsync(path, cancellationToken),
    cancellationToken,
    category: "Project");
```

For deliberately fire-and-forget work:

```csharp
Log.RunDetached(
    "Telemetry loop",
    cancellationToken => telemetry.RunAsync(cancellationToken),
    applicationCancellationToken,
    category: "Telemetry");
```

`RunDetached` observes and logs failures, then swallows them because no caller is awaiting the Task. Do not use it for work whose result or failure must affect application flow.

To show currently running operations:

```csharp
foreach (OperationSnapshot operation in Log.GetActiveOperations())
{
    Console.WriteLine(
        $"{operation.Name} - {operation.Id} - {operation.StartedAt}");
}
```

## Open the console later

For example, from a menu command:

```csharp
bool opened = Log.OpenConsole(replayRecentEntries: true);

if (!opened)
    Log.Warning("Could not allocate the log console.", "Logging");
```

On Windows this calls `AllocConsole`. On Linux and macOS it writes to the process's existing stdout; the supplied implementation does not launch a terminal emulator.

## Generated files

Default location:

- Windows: `%LOCALAPPDATA%\TitanControl\Logs`
- Other platforms: the platform value returned for `LocalApplicationData`, with an application-directory fallback

Normal file pattern:

```text
TitanControl-20260728-p12345.log
```

Emergency fallback file:

```text
TitanControl-emergency.log
```

The emergency file is written synchronously by last-chance exception handling before the logger waits for its normal queue.

## Important limitations

No in-process logger can guarantee a final write after power loss, `kill -9`, `Environment.FailFast`, a stack overflow, severe native memory corruption, or storage failure.

A logger also cannot discover exceptions that application code catches and silently ignores. Those exceptions must be passed to `Log.Error`/`Log.Warning`, or the operation must run through `Log.RunAsync`/`Log.RunDetached`.

Avoid using `AppDomain.FirstChanceException` as an "all caught exceptions" logger in production. It fires for every thrown exception, including exceptions that libraries intentionally use for control flow, and can create very high log volume or recursion.
