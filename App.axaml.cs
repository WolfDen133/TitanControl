using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using System.Diagnostics;
using TitanControl.Session;
using TitanControl.Session.Interface;

namespace TitanControl;

public partial class App : Application
{
    public static ISessionManager<TitanWebAPI.Titan> SessionManager { get; private set; } = default!;

    public override void Initialize()
    {
        SessionManager = new SessionManager<TitanWebAPI.Titan>
        (
            new SessionOptions()
        );

        var session = SessionManager.Create("Default Session");
        session.Start();

        session.StateChanged += (session, e) =>
        {
            Debug.WriteLine($"{e.CurrentState}");
        };

        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }
}