using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using TitanControl.ViewModels.Page;

namespace TitanControl.Views.Pages
{
    public partial class WorkspacePage : UserControl
    {
        public WorkspacePageModel Model
        {
            get
            {
                if (DataContext is not WorkspacePageModel m)
                    throw new InvalidOperationException($"Could not find valid data context for {nameof(WorkspacePage)}.");

                return m;
            }
        }

        public WorkspacePage()
        {
            InitializeComponent();
        }

        protected override void OnLoaded(RoutedEventArgs e)
        {
            base.OnLoaded(e);
        }
    }
}