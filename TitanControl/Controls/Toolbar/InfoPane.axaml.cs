using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using TitanControl.Controls.Models;
using TitanControl.Services.Session;

namespace TitanControl.Controls.Toolbar;

public partial class InfoPane : UserControl
{
    public InfoPane()
    {
        InitializeComponent();

        DataContext = new InfoModel();
    }

    public void UpdateStatus(SessionConnectionState state)
    {
        if (DataContext is not InfoModel) return;
        
        ((InfoModel)DataContext).UpdateSessionState(state);
    }

    public void UpdateWorkspace(string workspace)
    {

    }

    public void UpdateTitle(string title)
    {

    }
}