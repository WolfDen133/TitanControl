using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using ShimSkiaSharp.Editing;
using System;
using System.Collections.Generic;
using TitanControl.Events.Workspace;
using TitanControl.Logging;
using TitanControl.ViewModels.Workspace;
using TitanControl.Views.Controls.Layout.Grid;
using TitanControl.Views.Controls.Toolbar;
using TitanControl.Views.State;

namespace TitanControl.Views
{
    public partial class WorkspaceView : UserControl
    {
        private const string LoggingCategory = "WorkspaceView";

        public static readonly RoutedEvent<WorkspaceActionEventArgs> ActionChangedEvent =
            RoutedEvent.Register<WorkspaceView, WorkspaceActionEventArgs>(
                nameof(ActionChanged), 
                RoutingStrategies.Tunnel);

        private List<WorkspaceAction> _twoStepActions =
        [
            WorkspaceAction.Move,
            WorkspaceAction.Copy
        ];

        public WorkspaceViewModel Model
        {
            get
            {
                if (DataContext is not WorkspaceViewModel m)
                    throw new InvalidOperationException($"Could not find valid data context for {nameof(WorkspaceView)}.");

                return m;
            }
        }

        public event EventHandler<WorkspaceActionEventArgs> ActionChanged
        {
            add => AddHandler(ActionChangedEvent, value, RoutingStrategies.Tunnel);
            remove => RemoveHandler(ActionChangedEvent, value);
        }

        public WorkspaceView()
        {
            InitializeComponent();
        }

        protected override void OnLoaded(RoutedEventArgs e)
        {
            base.OnLoaded(e);

            Dispatcher.Post(() =>
            {
                Model.PropertyChanged += Model_PropertyChanged;
            }, DispatcherPriority.Loaded);
        }

        private void Model_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(Model.Action))
                return;

            RaiseEvent(new WorkspaceActionEventArgs(ActionChangedEvent, Model.Action));

            EnableSelectionAreaListener(false);
            EnableExclusiveSelection(false);
            PART_ControlGrid.ControlsSelected += ControlsSelected;

            if (Model.Action == WorkspaceAction.Add)
            {
                EnableExclusiveSelection();
                EnableSelectionAreaListener();

                PART_ControlGrid.ControlsSelected -= ControlsSelected;
                return;
            }

            if (Model.ActionAvailable)
            {
                Execute();
                return;
            }
        }

        private void EnableExclusiveSelection(bool value = true)
        {
            PART_ControlGrid.SnapSelection = value;
            PART_ControlGrid.SelectOver = !value;
        }

        private void ControlsSelected(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Dispatcher.Invoke(() => Execute());
            e.Handled = true;
        }

        private void PART_ControlGrid_SelectionCompleted(object? sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(() => Execute(true));
            e.Handled = true;
        }

        public void EnableSelectionAreaListener(bool enabled = true)
        {
            if (enabled) 
                PART_ControlGrid.SelectionCompleted += PART_ControlGrid_SelectionCompleted;
            else
                PART_ControlGrid.SelectionCompleted -= PART_ControlGrid_SelectionCompleted;
        }

        private void Execute(bool bypass = false)
        {
            if (PART_ControlGrid.SelectedArea.Equals(new Rect(0, 0, 0, 0))
                && (_twoStepActions.Contains(Model.Action)
                   || Model.Action.Equals(WorkspaceAction.Add)))
            {
                return;
            }

            if (_twoStepActions.Contains(Model.Action) && !bypass)
            {
                EnableSelectionAreaListener();
                EnableExclusiveSelection();
                return;
            }

            EnableSelectionAreaListener(false);
            EnableExclusiveSelection(false);
            PART_ControlGrid.SelectOver = false;

            Model.ExecuteAction(PART_ControlGrid.SelectedArea);

            if (!(Model.Latch && Model.Action == WorkspaceAction.Add))
                PART_ControlGrid.SelectOver = true;

            PART_ControlGrid.InvalidateArrange();
            PART_ControlGrid.InvalidateVisual();
        }
    }
}