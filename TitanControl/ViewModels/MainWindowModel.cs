using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TitanControl.Helper;
using TitanControl.Logging;
using TitanControl.Services.Session;
using TitanControl.Services.Workspace;
using TitanControl.ViewModels.Controls;
using TitanControl.ViewModels.Page;
using TitanControl.ViewModels.Page.HandleBrowser;
using TitanControl.ViewModels.Workspace;
using TitanControl.Views.Controls.Toolbar.Button;
using TitanControl.Views.Controls.Toolbar.Buttons;
using TitanControl.Views.Pages;

namespace TitanControl.ViewModel
{
    public class MainWindowModel : BaseViewModel
    {
        private static string LoggingCategory = "MainWindowModel";

        private bool _editMode = false;
        
        private IWorkspaceService _workspaceService;
        private ISessionService _sessionService;

        private PageId _currentPage = PageId.None;
        private bool _isGoingBack;

        public Dictionary<PageId, IPageModel> PageModels = new();

        public ToolbarModel ToolbarModel { get; private set; }
        public WorkspaceViewModel WorkspaceModel { get; private set; }

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
            WorkspaceModel = new WorkspaceViewModel(workspaceService, sessionService);
        }

        public void EnableEditMode(bool enable = true)
        {
            EditMode = enable;
        }

        public async Task RegisterPageModels()
        {
            await RegisterPageModel(PageId.Session, new SessionPageModel(_sessionService, _workspaceService));
            await RegisterPageModel(PageId.HandleBrowser, new HandleBrowserModel(_sessionService));
        }

        private async Task RegisterPageModel(PageId id, IPageModel model)
        {
            await model.InitializeAsync();

            PageModels.Add(id, model);
        }

        public override async Task InitializeAsync()
        {
            await LoadWorkspace();
            await RegisterPageModels();

            ToolbarModel.ButtonClicked += async (s, e) 
                => await HandlePageChange(e.ButtonId, e.ButtonAction);

            await WorkspaceModel.InitializeAsync();

            foreach (var page in PageModels.Values)
                await page.InitializeAsync();

            var session = _workspaceService.CurrentWorkspace.Options.Session;

            if (session != Guid.Empty)
                await _sessionService.Select(session);
        }

        private async Task HandlePageChange(ButtonId button, ButtonAction action)
        {
            var page = button switch
            {
                ButtonId.Sessions => PageId.Session,
                ButtonId.Assign => PageId.HandleBrowser,
                _ => PageId.None
            };

            await SetView(action != ButtonAction.ToggleUp ? page : PageId.None);
        }

        public async Task SetView(PageId page)
        {
            if (page == CurrentPage)
                return;

            if (page == PageId.None)
            {
                await SetPageVisible(CurrentPage, false);
                CurrentPage = page;
                return;
            }

            if (!PageModels.TryGetValue(page, out _))
            {
                var ex = new InvalidOperationException("Page not found.");
                Log.Error(ex, $"The page {page} does not exist in the dictionary.", LoggingCategory);
                throw ex;
            }

            if (CurrentPage != PageId.None)
                await SetPageVisible(CurrentPage, false);

            await SetPageVisible(page, true);
            CurrentPage = page;
        }

        public async Task SetPageVisible(PageId page, bool visible = false)
        {
            var selected = PageModels[page];

            if (visible)
                await selected.OnOpenAsync();
            else
                await selected.OnCloseAsync();
        }

        public async Task LoadWorkspace()
        {
            if (_workspaceService.HasLastWorkspace)
            {
                await _workspaceService.LoadAsync();
                return;
            }

            var name = PathHelper.GetNextFileName(
                "Untitled Workspace",
                _workspaceService.WorkspaceNames);

            await _workspaceService.Create(name);
        }
    }
}
