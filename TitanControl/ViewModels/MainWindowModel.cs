using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TitanControl.Helper;
using TitanControl.Logging;
using TitanControl.Services.Session;
using TitanControl.Services.Workspace;
using TitanControl.ViewModels.Controls;
using TitanControl.ViewModels.Page;
using TitanControl.Views.Controls.Toolbar.Button;
using TitanControl.Views.Controls.Toolbar.Buttons;

namespace TitanControl.ViewModel
{
    public class MainWindowModel : BaseViewModel
    {
        private static string LoggingCategory = "MainWindowModel";

        private bool _editMode = false;
        private Dictionary<PageId, IPage> _pages = new();

        private IWorkspaceService _workspaceService;
        private ISessionService _sessionService;

        private PageId _currentPage = PageId.Workspace;
        private bool _isGoingBack;

        public ToolbarModel ToolbarModel { get; private set; }

        public WorkspacePageModel WorkspacePage => (WorkspacePageModel)_pages[PageId.Workspace];
        public SessionPageModel SessionPage => (SessionPageModel)_pages[PageId.Session];

        public PageId CurrentPage
        {
            get => _currentPage;
            set => SetProperty(ref _currentPage, value);
        }

        public bool EditMode
        {
            get => _editMode;
            set => SetProperty(ref _editMode, value);
        }

        public bool IsGoingBack
        {
            get => _isGoingBack;
            set => SetProperty(ref _isGoingBack, value);
        }


        public MainWindowModel(IWorkspaceService workspaceService, ISessionService sessionService)
        {
            _workspaceService = workspaceService;
            _sessionService = sessionService;

            ToolbarModel = new ToolbarModel(sessionService, workspaceService);
        }

        public void EnableEditMode(bool enable = true)
        {
            EditMode = enable;
        }

        public void RegisterPageModels()
        {
            _pages.Add(PageId.Workspace, new WorkspacePageModel(_workspaceService, _sessionService));
            _pages.Add(PageId.Session, new SessionPageModel(_sessionService, _workspaceService));
        }

        public async Task Initialize()
        {
            RegisterPageModels();

            await LoadWorkspace();

            foreach (var page in _pages.Values)
                await page.InitializeAsync();

            ToolbarModel.ButtonClicked += 
                (_, args) => HandlePageChange(args.ButtonId, args.ButtonAction);

            SetView(PageId.Workspace);
        }

        private void HandlePageChange(ButtonId button, ToolbarButton.ButtonAction action)
        {
            switch (button)
            {
                case ButtonId.Sessions:
                    if (action == ToolbarButton.ButtonAction.ToggleDown)
                        SetView(PageId.Session);
                    else
                        SetView(PageId.Workspace);
                    break;
            }
        }

        public void SetView(PageId page)
        {
            if (!_pages.TryGetValue(page, out IPage? value))
            {
                var ex = new InvalidOperationException("Page not found.");
                Log.Error(ex, $"The page {page} does not exist in the dictionary.", LoggingCategory);
                throw ex;
            }

            CurrentPage = page;
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
