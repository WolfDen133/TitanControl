using Avalonia;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using Humanizer;
using ShimSkiaSharp.Editing;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using TitanControl.Logging;
using TitanControl.Models.Control;
using TitanControl.Models.Workspace;
using TitanControl.Services.Session;
using TitanControl.Services.Workspace;
using TitanControl.ViewModel;
using TitanControl.ViewModels.Controls.Toolbar;
using TitanControl.ViewModels.Page;
using TitanControl.ViewModels.Workspace.Handle;
using TitanControl.Views.Controls.Handle;
using TitanControl.Views.Controls.Toolbar.Button;
using TitanControl.Views.State;
using TitanControl.WebAPI;
using TitanControl.WebAPI.Data;

namespace TitanControl.ViewModels.Workspace
{
    public class WorkspaceViewModel : BaseViewModel, IAsyncDisposable
    {
        private const string LoggingCategory = "Workspace ViewModel";

        private readonly IWorkspaceService _workspaceService;
        private readonly ISessionService _sessionService;
        private readonly ToolbarModel _toolbar;

        public bool ActionAvailable = false;
        private bool _latch = false;

        private WorkspaceAction _action = WorkspaceAction.None;
        private HandleControlId _addingControlType;

        public bool IsSelecting => Action != WorkspaceAction.None;

        public WorkspaceAction Action 
        { 
            get => _action;
            set
            {
                SetProperty(ref _action, value);
                OnPropertyChanged(nameof(IsSelecting));    
            }
        }

        public ObservableCollection<IHandleControl> SelectedControls { get; set; } = [];

        public ObservableCollection<IHandleControl> Controls { get; } = [];

        public WorkspaceModel CurrentWorkspace => _workspaceService.CurrentWorkspace;

        public event EventHandler<WorkspaceAction>? ExecuteAvailable;

        public WorkspaceViewModel(IWorkspaceService workspaceService, 
            ISessionService sessionService, 
            ToolbarModel toolbarModel)
        {
            _workspaceService = workspaceService;
            _sessionService = sessionService;
            _toolbar = toolbarModel;
        }

        public override async Task InitializeAsync()
        {
            foreach (var model in CurrentWorkspace.Controls)
                AddControl(model.ToInstance<IHandleControl>(_sessionService));

            SelectedControls.CollectionChanged += SelectedControls_CollectionChanged;

            Log.Information($"Loaded {Controls.Count} controls into workspace", LoggingCategory);
        }       

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.PropertyName != nameof(Action))
                return;
            
            if (SelectedControls.Any() && Action == WorkspaceAction.None)
            {
                _toolbar.ShowAvailable();
                ActionAvailable = true;
                return;
            }

            _toolbar.ShowAvailable(false);
            ActionAvailable = false;
        }

        private void SelectedControls_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (SelectedControls.Any() && Action == WorkspaceAction.None)
            {
                _toolbar.ShowAvailable();
                ActionAvailable = true;
                return;
            }

            _toolbar.ShowAvailable(false);
            ActionAvailable = false;
        }

        public void ClearSelection()
        {
            foreach (var control in SelectedControls)
                control.IsSelected = false;

            SelectedControls.Clear();
        }

        public void ToolButtonClicked(ButtonId id, ButtonAction action)
        {
            if (id == ButtonId.Latch)
            {
                _latch = action == ButtonAction.ToggleDown ? true : false;
                return;
            }

            var workspaceAction = id switch
            {
                ButtonId.AddButton => WorkspaceAction.Add,
                ButtonId.AddFader => WorkspaceAction.Add,
                ButtonId.Remove => WorkspaceAction.Remove,
                ButtonId.Assign => WorkspaceAction.Assign,
                ButtonId.Copy => WorkspaceAction.Copy,
                ButtonId.Move => WorkspaceAction.Move,
                ButtonId.Options => WorkspaceAction.Options,
                _ => WorkspaceAction.None
            };

            _addingControlType = id switch
            {
                ButtonId.AddButton => HandleControlId.Button,
                ButtonId.AddFader => HandleControlId.Fader,
                _ => HandleControlId.None
            };

            Action = action switch
            {
                ButtonAction.ToggleDown => workspaceAction,
                ButtonAction.ToggleUp => WorkspaceAction.None,
                _ => WorkspaceAction.None
            };

            Log.Debug($"Toolbar button clicked: {id}, Action: {action}", LoggingCategory);
        }


        private void AddControl(IHandleControl control)
        {
            Controls.Add(control);
        }

        private void RemoveControl(IHandleControl control)
        {
            Controls.Remove(control);
        }

        public void ExecuteAction(Rect? args = null)
        {
            if (Action == WorkspaceAction.None)
                return;

            switch (Action)
            {
                case WorkspaceAction.Add:
                    Add((Rect)args!);
                    break;

                case WorkspaceAction.Copy:
                    Copy((Rect)args!);
                    break;

                case WorkspaceAction.Move:
                    Move((Rect)args!);
                    break;

                case WorkspaceAction.Assign:
                    Assign();
                    break;

                case WorkspaceAction.Options:
                    Options();
                    break;

                case WorkspaceAction.Remove:
                    Remove();
                    break;
            }

            ClearSelection();

            var old = Action;
            Action = WorkspaceAction.None;

            if (_latch)
            {
                Action = old;
                return;
            }

            var button = old switch
            {
                WorkspaceAction.Add => _addingControlType switch
                {
                    HandleControlId.Button => ButtonId.AddButton,
                    HandleControlId.Fader => ButtonId.AddFader,
                    HandleControlId.ColorPicker => ButtonId.AddColorPicker,
                    _ => ButtonId.None,
                },
                WorkspaceAction.Copy => ButtonId.Copy,
                WorkspaceAction.Move => ButtonId.Move,
                WorkspaceAction.Assign => ButtonId.Assign,
                WorkspaceAction.Options => ButtonId.Options,
                WorkspaceAction.Remove => ButtonId.Remove,
                _ => ButtonId.None
            };

            if (button != ButtonId.None)
                _toolbar.ReleaseToggleSoft(button);
        }

        private void Add(Rect at)
        {
            ControlModel controlModel = _addingControlType switch
            {
                HandleControlId.Fader => new FaderControlModel(),
                HandleControlId.Button => new ButtonControlModel(),
                _ => throw new ArgumentOutOfRangeException(nameof(_addingControlType), $"No control type defined for {_addingControlType}")
            };

            controlModel.Location = 
                new Rectangle(
                    (int)at.X, 
                    (int)at.Y, 
                    (int)at.Width, 
                    (int)at.Height);

            CurrentWorkspace.Controls.Add(controlModel);

            AddControl(controlModel.ToInstance<IHandleControl>(_sessionService));

            Log.Information($"Added new control of type {_addingControlType} to workspace {CurrentWorkspace.Name}", LoggingCategory);
        }

        private void Remove()
        {
            foreach (var control in SelectedControls)
            {
                control.IsSelected = false;

                CurrentWorkspace.Controls.Remove((ControlModel)control.Model);
                RemoveControl(control);
            }

            Log.Information($"Removed selected controls from workspace {CurrentWorkspace.Name}", LoggingCategory);
        }

        private void Assign()
        {
            // Action null check
            if (Action == WorkspaceAction.None
                || SelectedControls.Count == 0)
                return;

            // Open the assign page for the selected controls
            // Assign the selected controls to the returned titan handle information
            // Clear selected control list and reset action
            // Log
        }

        private void Copy(Rect to)
        {
            // Action null check
            if (Action == WorkspaceAction.None 
                || SelectedControls.Count == 0)
                return;

            // TODO Calculations of multiple
            var control = SelectedControls.FirstOrDefault()!.Copy();

            control.Location
                = new Rectangle(
                    (int)to.X,
                    (int)to.Y,
                    (int)to.Width,
                    (int)to.Height);

            CurrentWorkspace.Controls.Add((ControlModel)control.Model);

            AddControl(control);

            Log.Information($"Copied {control.GetType().Name} to {to.ToString()} in workspace {CurrentWorkspace.Name}.", LoggingCategory);
        }

        private void Move(Rect to)
        {
            if (Action == WorkspaceAction.None
                || SelectedControls.Count == 0)
                return;

            // TODO Calculations of multiple
            var control = SelectedControls.FirstOrDefault()!;

            control!.Location 
                = new Rectangle(
                    (int)to.X,
                    (int)to.Y,
                    (int)to.Width,
                    (int)to.Height);
            
            Log.Information($"Moved {control.GetType().Name} to {to} in workspace {CurrentWorkspace.Name}.", LoggingCategory);
        }
        private void Options()
        {
            if (Action == WorkspaceAction.None
                || SelectedControls.Count == 0)
                return;

            // Open the options page for the selected controls
            // And make changes
            // Clear selected control list and reset action
        }

        public ValueTask DisposeAsync()
        {
            Controls.Clear();

            return ValueTask.CompletedTask;
        }
    }
}
