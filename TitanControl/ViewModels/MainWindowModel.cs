using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TitanControl.Events.Workspace;
using TitanControl.Helper;
using TitanControl.Logging;
using TitanControl.Services.Session;
using TitanControl.Services.Workspace;
using TitanControl.ViewModels.Controls.Toolbar;
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

        private readonly List<PageId> _pageHistory = new();
        private readonly List<PageId> _requestedPages = new();

        private bool _editMode = false;
        private bool _isGoingBack;
        
        private PageId _currentPage = PageId.None;
        private IWorkspaceService _workspaceService;
        private ISessionService _sessionService;

        public Dictionary<PageId, IPageModel> PageModels = new();
        public ToolbarModel ToolbarModel { get; private set; }
        public WorkspaceViewModel WorkspaceModel { get; private set; }

        public event EventHandler<IPageModel>? PageRegistered;

        public PageId CurrentPage
        {
            get => _currentPage;
            set => SetProperty(ref _currentPage, value);
        }

        public bool EditMode
        {
            get => _editMode;
            set {
                SetProperty(ref _editMode, value);
            }
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
            WorkspaceModel = new WorkspaceViewModel(workspaceService, sessionService, ToolbarModel);
        }


        public override async Task InitializeAsync()
        {
            await LoadWorkspace();
            await RegisterPageModels();

            await WorkspaceModel.InitializeAsync();
            WorkspaceModel.RequestPage += OnRequestPage;

            var session = _workspaceService.CurrentWorkspace.Options.Session;

            if (session != Guid.Empty)
                await _sessionService.Select(session);
        }

        private async void OnRequestPage(object? sender, PageRequestedEventArgs e)
        {
            if (e.Page == PageId.None)
                return;

            if (e.Opening)
                await NavigateTo(e.Page);
            else
                await NavigateAway(e.Page);
        }

        public void EnableEditMode(bool enable = true)
        {
            EditMode = enable;
        }

        public async Task RegisterPageModels()
        {
            await RegisterPageModel(
                PageId.Session,
                new SessionPageModel(_sessionService, _workspaceService));

            await RegisterPageModel(
                PageId.HandleBrowser,
                new HandleBrowserModel(_sessionService,
                async (cancelled) =>
                {
                    if (!cancelled)
                    {
                        WorkspaceModel.HandleActionCompleted();
                        await NavigateAway(PageId.HandleBrowser);
                        return;
                    }

                    WorkspaceModel.HandleActionCancel(WorkspaceAction.Assign);
                }));
        }

        private async Task RegisterPageModel(PageId id, IPageModel model)
        {
            await model.InitializeAsync();

            PageModels.Add(id, model);

            PageRegistered?.Invoke(this, model);
        }

        public async Task OnToolButtonClicked(ButtonId button, ButtonAction action)
        {
            var page = button switch
            {
                ButtonId.Sessions => PageId.Session,
                _ => PageId.None
            };

            if (page == PageId.None)
            {
                WorkspaceModel.ToolButtonClicked(button, action);
                return;
            }

            switch (action)
            {
                case ButtonAction.ToggleDown:
                    await NavigateTo(page);
                    break;
                case ButtonAction.ToggleUp:
                    await NavigateAway(page);
                    break;
            }
        }

        private async Task NavigateTo(PageId id)
        {
            if (!PageModels.ContainsKey(id))
                throw new InvalidOperationException($"No page found for {id}.");

            // Already on top.
            if (_pageHistory.Count > 0 &&
                _pageHistory[^1] == id)
                return;

            // Close the currently displayed page.
            if (_pageHistory.Count > 0)
            {
                var currentId = _pageHistory[^1];

                if (PageModels.ContainsKey(currentId))
                    await SetPageVisible(currentId, false);
            }

            // Avoid duplicate history entries.
            _pageHistory.Remove(id);
            _pageHistory.Add(id);

            await SetPageVisible(id, true);
        }

        private async Task NavigateAway(PageId id)
        {
            int index = _pageHistory.IndexOf(id);

            if (index < 0)
                return;

            bool isCurrentPage =
                index == _pageHistory.Count - 1;

            // Remove THIS page, regardless of where it is.
            _pageHistory.RemoveAt(index);

            // It wasn't the visible page.
            // Nothing visually needs to change.
            if (!isCurrentPage)
                return;

            if (PageModels.ContainsKey(id))
                await SetPageVisible(id, false);

            // Reveal whatever was underneath.
            if (_pageHistory.Count > 0)
            {
                var previousId = _pageHistory[^1];

                if (PageModels.ContainsKey(previousId))
                    await SetPageVisible(previousId, true);
            }
        }


        // Unused reserved for commands
        private async Task NavigateBack()
        {
            if (_pageHistory.Count == 0)
                return;

            // Current page is always the last item.
            PageId currentId = _pageHistory[^1];

            _pageHistory.RemoveAt(_pageHistory.Count - 1);

            if (PageModels.ContainsKey(currentId))
                await SetPageVisible(currentId, false);

            // Nothing underneath -> back to workspace.
            if (_pageHistory.Count == 0)
                return;

            PageId previousId = _pageHistory[^1];

            if (PageModels.ContainsKey(previousId))
                await SetPageVisible(previousId, true);
        }


        public async Task SetPageVisible(PageId page, bool visible = false)
        {
            var selected = PageModels[page];
             
            if (visible)
                await selected.OnOpenAsync();
            else
                await selected.OnCloseAsync();

            Log.Debug($"Set {page} to {(visible ? "active" : "inactive")}", LoggingCategory);
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
