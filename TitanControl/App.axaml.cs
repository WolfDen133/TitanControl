using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Logging;
using Avalonia.Markup.Xaml;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using TitanControl.Disk;
using TitanControl.Helper;
using TitanControl.Logging;
using TitanControl.Services.Session;
using TitanControl.Services.Workspace;
using TitanControl.ViewModels;
using TitanControl.WebAPI;

namespace TitanControl;

public partial class App : Application
{
    private ResourceHelper? _resourceHelper;
    private IDisposable? _dispatcherLogging;

    private IWorkspaceService _workspaceService = null!;
    private ISessionService _sessionService = null!;
    private FileHandler _fileHandler = null!;

    public override void Initialize()
    {
        _resourceHelper = new(this);
        AvaloniaXamlLoader.Load(this);

        if (Design.IsDesignMode)
        {
            Log.InitializeDesign();
            return;
        }
        
        RegisterSystems();
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
            desktop.MainWindow = new MainWindow();

            _ = RegisterSystems();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private async Task RegisterSystems()
    {
        Log.Information("Initializing TitanControl Application", "Application");

        _workspaceService = new WorkspaceService(
            new Disk.Resporitory.Workspace.WorkspaceRepository()
            );
    }
}