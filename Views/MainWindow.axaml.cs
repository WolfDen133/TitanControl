using Avalonia.Controls;

namespace TitanControl;

public partial class MainWindow : Window
{
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