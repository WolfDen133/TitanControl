using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using TitanControl.Disk.Model.Workspace;
using TitanControl.Workspaces;

namespace TitanControl.Views.Page.Pages
{
    public partial class WorkspacePage : BasePage
    {
        public WorkspacePage()
        {
            InitializeComponent();
        }

        public override PageId Id => PageId.Workspace;

        public void LoadControls(Workspace workspace)
        {
            // TODO
            foreach (var control in workspace.Controls)
            {
                PART_ControlGrid.AddControl(control);
            }
        }
    }
}