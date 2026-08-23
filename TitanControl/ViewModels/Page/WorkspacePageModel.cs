using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using TitanControl.Controls.Handle;
using TitanControl.Controls.Models.Handle;
using TitanControl.Logging;
using TitanControl.Models;
using TitanControl.Models.Control;
using TitanControl.Models.Workspace;
using TitanControl.Services.Session;
using TitanControl.Services.Workspace;

namespace TitanControl.ViewModels.Page
{
    public class WorkspacePageModel : BaseViewModel, IPage
    {
        private const string LoggingCategory = "Workspace ViewModel";

        private readonly IWorkspaceService _workspaceService;
        private readonly ISessionService _sessionService;

        public PageId Id => PageId.Workspace;

        public ObservableCollection<IHandleControl> Controls { get; } = [];

        public WorkspaceModel CurrentWorkspace => _workspaceService.CurrentWorkspace;


        public WorkspacePageModel(IWorkspaceService workspaceService, ISessionService sessionService)
        {
            _workspaceService = workspaceService;
            _sessionService = sessionService;
        }

        public async Task InitializeAsync()
        {
            var buttonModel = new ButtonControlModel()
            {
                Location = new(0, 0, 2, 2)
            };

            var faderModel = new FaderControlModel()
            {
                Location = new(2, 0, 2, 2)
            };


            Controls.Add(buttonModel.ToInstance<IHandleControl>(_sessionService));
            Controls.Add(faderModel.ToInstance<IHandleControl>(_sessionService));

            foreach (var model in CurrentWorkspace.Controls)
                Controls.Add((IHandleControl)model.ToInstance(_sessionService));

            Log.Information($"Loaded {Controls.Count} controls into workspace", LoggingCategory);
        }
    }
}
