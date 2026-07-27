using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using TitanControl.Controls.Models;

namespace TitanControl.Controls.Toolbar;

public partial class InfoPane : UserControl
{
    public InfoPane()
    {
        InitializeComponent();

        DataContext = new InfoModel();
    }
}