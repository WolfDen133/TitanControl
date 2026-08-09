using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using System;
using System.Diagnostics;
using TitanControl.Logging;
using TitanControl.Session;
using TitanControl.Session.Interface;
using Avalonia.Controls;
using TitanControl.ViewModels;
using TitanControl.WebAPI;
using TitanControl.Helper;
using Avalonia.Logging;
using TitanControl.Workspace;
using TitanControl.Disk;

namespace TitanControl;

public partial class App : Application
{
    private ResourceHelper? _resourceHelper;
    private IDisposable? _dispatcherLogging;
    public static SessionManager<Titan> SessionManager { get; private set; } = default!;
    public static WorkspaceManager WorkspaceManager { get; private set; } = default!;

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

    private void RegisterSystems()
    {
        Log.Information("Initializing TitanControl Application", "Application");

        FileHandler.Initialize();

        SessionManager = new SessionManager<TitanControl.WebAPI.Titan>
        (
            new SessionOptions()
        );

        WorkspaceManager = new WorkspaceManager();
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
        }

        base.OnFrameworkInitializationCompleted();
    }
}