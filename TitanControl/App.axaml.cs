using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using System;
using System.Threading.Tasks;
using TitanControl.Disk;
using TitanControl.Disk.Resporitory.Session;
using TitanControl.Disk.Resporitory.Workspace;
using TitanControl.Helper;
using TitanControl.Logging;
using TitanControl.Services.Dialog;
using TitanControl.Services.Session;
using TitanControl.Services.Workspace;
using TitanControl.ViewModel;
using TitanControl.Views;

namespace TitanControl;

public partial class App : Application
{
    private const string LoggingCategory = "Application";

    private ResourceHelper? _resourceHelper;
    private IDisposable? _dispatcherLogging;
    public static DialogService DialogService
    {
        get;
        private set;
    } = null!;

    public override void Initialize()
    {
        _resourceHelper = new(this);
        AvaloniaXamlLoader.Load(this);

        if (Design.IsDesignMode)
        {
            Log.InitializeDesign();
            return;
        }
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (!Design.IsDesignMode)
        {
            _dispatcherLogging = AvaloniaLogging.InstallDispatcherExceptionLogging(true);
            Log.Information("Opening main window", LoggingCategory);
        }

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            _ = StartAsync(desktop);
        }

        base.OnFrameworkInitializationCompleted();
    }

    private async Task StartAsync(IClassicDesktopStyleApplicationLifetime desktop)
    {
        Log.Information("Initializing TitanControl Application", LoggingCategory);
        var fileHandler = new FileHandler();
        var workspaceService = new WorkspaceService(new WorkspaceRepository(fileHandler));
        var sessionService = new SessionService(new SessionRepository(fileHandler), workspaceService);
        var mainWindowModel = new MainWindowModel(workspaceService, sessionService);

        try
        {
            Log.Debug("Initialising TitanControl window...", LoggingCategory);
            var mainWindow = new MainWindow
            {
                DataContext = mainWindowModel
            };

            Log.Information("Opening TitanControl window...", LoggingCategory);
            desktop.MainWindow = mainWindow;
            mainWindow.Show();

            Log.Debug("Initializing file handler...", LoggingCategory);
            await fileHandler.InitializeAsync();

            Log.Debug("Initialising services...", LoggingCategory);
            DialogService = new DialogService(mainWindow);

            await workspaceService.InitializeAsync();
            await sessionService.InitializeAsync();

            Log.Debug("Initializing view models...", LoggingCategory);
            await mainWindowModel.InitializeAsync();

            mainWindow.InitializationCompleted();
            Log.Information("TitanControl started successfully!", "Application");
            
        }
        catch (Exception ex)
        {
            Log.Error(ex, "A critical error has occured during application initialization, terminating...", "Application");
            desktop.Shutdown(1);
        }
    }

}