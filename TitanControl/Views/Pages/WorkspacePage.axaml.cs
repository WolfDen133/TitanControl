using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using TitanControl.ViewModels;
using TitanControl.ViewModels.Page;

namespace TitanControl.Views.Pages
{
    public partial class WorkspacePage : UserControl, IPage
    {
        public WorkspacePage()
        {
            InitializeComponent();
        }

        public PageId Id => PageId.Workspace;
    }
}