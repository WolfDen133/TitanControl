using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using System.Collections.ObjectModel;
using System.Diagnostics;
using TitanControl.Logging;
using TitanControl.ViewModels;
using TitanControl.Views.Page;
using TitanControl.Views.Page.Pages;

namespace TitanControl;

public partial class MainWindow : Window
{
    public static PageManager PageManager { get; private set; } = new();

    public MainWindow()
    {
        InitializeComponent();

        DataContext = new MainWindowModel();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        if (DataContext is not MainWindowModel model || Design.IsDesignMode) return;

        PageManager.Initialize(PART_ContentGrid);
        model.Initialize();

        Log.Debug("MainWindow initialized.", "MainWindow");

        model.CurrentSession.StateChanged += (sender, args) =>
        {
            Dispatcher.UIThread.Invoke(() => PART_Toolbar.UpdateSessionStatus(args.CurrentState));
        };

        PageManager.ShowPage(PageId.Workspace);
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        PART_Toolbar.DoResize((int)e.NewSize.Width);

        base.OnSizeChanged(e);
    }
}