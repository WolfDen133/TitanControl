using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TitanControl.Controls.Models;
using TitanControl.Controls.Toolbar.Buttons;
using TitanControl.Helper;
using TitanControl.Logging;
using TitanControl.Services.Session;
using TitanControl.Services.Workspace;
using TitanControl.ViewModels.Page;

namespace TitanControl.ViewModels
{
    public class MainWindowModel : BaseViewModel
    {
        private static string LoggingCategory = "MainWindowModel";

        public ToolbarModel ToolbarModel { get; private set; }

        private IWorkspaceService _workspaceService;
        private ISessionService _sessionService;

        private Dictionary<PageId, IPage> _pages = new();

        private ObservableObject? _currentPage;

        public ObservableObject? CurrentPage
        {
            get => _currentPage;
            set => SetProperty(ref _currentPage, value);
        }

        public MainWindowModel(IWorkspaceService workspaceService, ISessionService sessionService)
        {
            _workspaceService = workspaceService;
            _sessionService = sessionService;

            ToolbarModel = new ToolbarModel(sessionService, workspaceService);

            RegisterPageModels();
        }

        public void RegisterPageModels()
        {
            _pages.Add(PageId.Workspace, new WorkspacePageModel(_workspaceService, _sessionService));
            _pages.Add(PageId.Session, new SessionPageModel(_sessionService, _workspaceService));
        }

        public async Task Initialize()
        {
            await LoadWorkspace();

            foreach (var page in _pages.Values)
                await page.InitializeAsync();


            ToolbarModel.ButtonClicked += (_, args) =>
            {
                switch (args.ButtonId)
                {
                    case ButtonId.Sessions:
                        if (args.ButtonAction == ToolbarButton.ButtonAction.ToggleDown)
                            SetView(PageId.Session);
                        else
                            SetView(PageId.Workspace);
                        break;
                }
            };

            SetView(PageId.Workspace);
        }

        public void SetView(PageId page)
        {
            if (!_pages.TryGetValue(page, out IPage? value))
            {
                var ex = new InvalidOperationException("Page not found.");
                Log.Error(ex, $"The page {page} does not exist in the dictionary.", LoggingCategory);
                throw ex;
            }

            CurrentPage = (ObservableObject)_pages[page];
        }

        public async Task LoadWorkspace()
        {
            if (_workspaceService.HasLastWorkspace)
                await _workspaceService.LoadAsync();
            else
                await _workspaceService.Create(PathHelper.GetNextFileName("Untitled Workspace", _workspaceService.WorkspaceNames));
        }
    }
}
