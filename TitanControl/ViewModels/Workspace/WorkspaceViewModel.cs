using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using TitanControl.Logging;
using TitanControl.Models.Control;
using TitanControl.Models.Workspace;
using TitanControl.Services.Session;
using TitanControl.Services.Workspace;
using TitanControl.ViewModel;
using TitanControl.ViewModels.Page;
using TitanControl.Views.Controls.Handle;
using TitanControl.WebAPI.Data;

namespace TitanControl.ViewModels.Workspace
{
    public class WorkspaceViewModel : BaseViewModel, IAsyncDisposable
    {
        private const string LoggingCategory = "Workspace ViewModel";

        private readonly IWorkspaceService _workspaceService;
        private readonly ISessionService _sessionService;

        public WorkspaceAction Action { get; }
        public ControlId AddControl = ControlId.None;

        public ObservableCollection<IHandleControl> Controls { get; } = [];

        public WorkspaceModel CurrentWorkspace => _workspaceService.CurrentWorkspace;

        public WorkspaceViewModel(IWorkspaceService workspaceService, ISessionService sessionService)
        {
            _workspaceService = workspaceService;
            _sessionService = sessionService;
        }

        public override async Task InitializeAsync()
        {
            foreach (var model in CurrentWorkspace.Controls)
                Controls.Add((IHandleControl)model.ToInstance(_sessionService));

            Log.Information($"Loaded {Controls.Count} controls into workspace", LoggingCategory);
        }

        public ValueTask DisposeAsync()
        {
            Controls.Clear();

            return ValueTask.CompletedTask;
        }
    }
}
