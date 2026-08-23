using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using System;
using TitanControl.Logging;
using TitanControl.ViewModels;

namespace TitanControl;

public partial class MainWindow : Window
{
    public MainWindowModel Model
    {
        get
        {
            if (DataContext is not MainWindowModel m)
                throw new InvalidOperationException($"Could not find valid data context for {nameof(MainWindow)}.");

            return m;
        }
    }

    public MainWindow()
    {
        InitializeComponent();
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        PART_Toolbar.DoResize((int)e.NewSize.Width);

        base.OnSizeChanged(e);
    }
}