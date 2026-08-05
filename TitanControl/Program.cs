using Avalonia;
using Avalonia.Svg.Skia;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using TitanControl.Helper;
using TitanControl.Logging;

namespace TitanControl;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        Log.Initialize(new LoggerOptions
        {
            MinimumLevel = AppConstants.Debug ? LogLevel.Debug : LogLevel.Warning,

            FlushInterval = TimeSpan.FromSeconds(5),
            LogDirectory = Path.Combine(PathHelper.AppDataPath, "logs"),

            ImmediateFlushLevel = LogLevel.Error,
            DurableFlushOnCritical = true,

            InstallGlobalExceptionHandlers = true,
            ObserveUnobservedTaskExceptions = true,

            OpenConsoleOnStart = AppConstants.Debug,
            WriteToConsole = true,

            RecentEntryCapacity = 500,
            RetainedFileCount = 10,

            FirstChanceExceptionFilter = exception => exception is InvalidCastException,
            CaptureFirstChanceExceptions = true
        });

        try
        {
            Log.Information(
                $"Starting TitanControl {AppConstants.AppVersion}...",
                category: "Startup");

            GC.KeepAlive(typeof(SvgImageExtension).Assembly);
            GC.KeepAlive(typeof(Avalonia.Svg.Skia.Svg).Assembly);
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }
        catch (Exception exception)
        {
            Log.Critical(
                exception,
                "TitanControl terminated unexpectedly.",
                category: "Program");

            Environment.ExitCode = 1;
        }
        finally
        {
            Log.Shutdown();
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
