using Avalonia.Controls;
using System;
using TitanControl.ViewModels.Workspace;

namespace TitanControl.Views
{
    public partial class WorkspaceView : UserControl
    {
        public WorkspaceViewModel Model
        {
            get
            {
                if (DataContext is not WorkspaceViewModel m)
                    throw new InvalidOperationException($"Could not find valid data context for {nameof(WorkspaceView)}.");

                return m;
            }
        }

        public WorkspaceView()
        {
            InitializeComponent();
        }
    }
}