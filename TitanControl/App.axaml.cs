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
            Log.Information("Opening main window", "Application");
        }

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            _ = StartAsync(desktop);
        }

        base.OnFrameworkInitializationCompleted();
    }

    private async Task StartAsync(IClassicDesktopStyleApplicationLifetime desktop)
    {
        Log.Information("Initializing TitanControl Application", "Application");

        try
        {
            Log.Debug("Initialising file handler", "Application");
            var fileHandler = new FileHandler();
            await fileHandler.InitializeAsync();

            Log.Debug("Initialising workspace service", "Application");
            var workspaceService = new WorkspaceService(new WorkspaceRepository(fileHandler));
            await workspaceService.InitializeAsync();

            Log.Debug("Initialising session service", "Application");
            var sessionService = new SessionService(new SessionRepository(fileHandler), workspaceService);
            await sessionService.InitializeAsync();

            Log.Debug("Initialising MainWindowModel", "Application");
            var mainWindowModel = new MainWindowModel(workspaceService, sessionService);
            await mainWindowModel.InitializeAsync();

            Log.Debug("Initialising MainWindow", "Application");
            var mainWindow = new MainWindow
            {
                DataContext = mainWindowModel
            };

            Log.Debug("Initialising Dialog Service", "Application");
            DialogService = new DialogService(mainWindow);

            Log.Debug("Opening window", "Application");
            desktop.MainWindow = mainWindow;
            mainWindow.Show();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "A critical error has occured during application initialization, terminating...", "Application");
            desktop.Shutdown(1);
        }
    }

}