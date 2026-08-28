using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using TitanControl.ViewModels.Page;

namespace TitanControl.Views.Pages
{
    public partial class HandleBrowserPage : BasePage
    {
        public override PageId Id => PageId.HandleBrowser;
        public override Dock Dock => Dock.Bottom;

        public HandleBrowserPage()
        {
            InitializeComponent();
        }
    }
}