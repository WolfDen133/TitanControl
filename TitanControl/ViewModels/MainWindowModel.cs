using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TitanControl.Events;
using TitanControl.Events.Workspace;
using TitanControl.Helper;
using TitanControl.Logging;
using TitanControl.Services.Command;
using TitanControl.Services.Dialog;
using TitanControl.Services.Session;
using TitanControl.Services.Workspace;
using TitanControl.ViewModels.Controls.Toolbar;
using TitanControl.ViewModels.Page;
using TitanControl.ViewModels.Page.HandleBrowser;
using TitanControl.ViewModels.Workspace;
using TitanControl.Views.Controls.Toolbar.Button;
using TitanControl.Views.Controls.Toolbar.Buttons;
using TitanControl.Views.Pages;
using TitanControl.WebAPI.Data.Model;

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
        public event EventHandler<bool>? RequestSplash;

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

        public string VersionString => $"Version {AppConstants.AppVersion}";

        public MainWindowModel(
            IWorkspaceService workspaceService, 
            ISessionService sessionService,
            ICommandService commandService)
        {
            _workspaceService = workspaceService;
            _sessionService = sessionService;

            ToolbarModel = new ToolbarModel(sessionService, workspaceService);
            WorkspaceModel = new WorkspaceViewModel(workspaceService, sessionService, commandService, ToolbarModel);
        }


        public override async Task InitializeAsync()
        {
            await LoadWorkspace();
            await WorkspaceModel.InitializeAsync();
            await RegisterPageModels();

            WorkspaceModel.RequestPage += OnRequestPage;
            
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

        public async void OnPageDialogClosed(object? sender, DialogClosedEventArgs args)
        {
            if (sender is not IPageModel page)
                return;

            await NavigateAway(page.Id);
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

            var handleBrowser = new HandleBrowserModel(_sessionService, WorkspaceModel);
            handleBrowser.DialogClosed += HandleBrowser_DialogClosed;

            await RegisterPageModel(PageId.HandleBrowser, handleBrowser);
        }

        private void HandleBrowser_DialogClosed(object? sender, DialogClosedEventArgs e)
        {
            if (e.Result is not List<Handle> handles)
                return;

            Log.Debug($"{handles.Any()}");

            WorkspaceModel.ConfirmAssign(handles);
        }

        private async Task RegisterPageModel(PageId id, IPageModel model)
        {
            await model.InitializeAsync();

            if (model is IDialog)
                ((IDialog)model).DialogClosed += OnPageDialogClosed;

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

                if (ToolbarModel.SystemButtons.Any(b => b.Id == button)
                    && button != ButtonId.Back)
                        await ExecuteSystemAction(button);
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

        private async Task ExecuteSystemAction(ButtonId id)
        {
            string? result;
            string? name;
            string? path;

            switch (id)
            {
                case ButtonId.Save:
                    result = await _workspaceService.SaveAsync();

                    await App.DialogService.ShowMessageAsync("Workspace Saved",
                        $"The workspace '{_workspaceService.CurrentWorkspace.Name}' has been saved to:\n{_workspaceService.CurrentWorkspace.Name}");

                    break;
                case ButtonId.SaveAs:
                    path = await App.DialogService.ShowSaveFileAsync(
                        $"Save {_workspaceService.CurrentWorkspace.Name} to disk", 
                        _workspaceService.CurrentWorkspace.Name + ".tcw");

                    if (path is null)
                        return;

                    // Needed to stop workspace overwriting and false loading
                    _workspaceService.CurrentWorkspace.ReasignId();

                    result = await _workspaceService.SaveAsync(_workspaceService.CurrentWorkspace, path);

                    if (result == null)
                    {
                        Log.Error($"The file for {_workspaceService.CurrentWorkspace.Name} was not able to be saved.", LoggingCategory);
                        return;
                    }

                    await App.DialogService.ShowMessageAsync("Workspace Saved",
                        $"The current workspace '{_workspaceService.CurrentWorkspace.Name}' has been saved to:\n{result}");

                        break;
                case ButtonId.Rename:
                    var old = _workspaceService.CurrentWorkspace.Name;
                    name = await App.DialogService.ShowTextAsync("Rename Workspace", $"Enter a new name for the workspace: {old}.");

                    if (name == null) return;

                    _workspaceService.CurrentWorkspace.Name = name;

                    result = await _workspaceService.RenameAsync(_workspaceService.CurrentWorkspace);

                    if (result == null)
                    {
                        Log.Warning($"No file found for {old} therefore created a new save.", LoggingCategory);

                        await App.DialogService.ShowMessageAsync("Workspace Saved",
                            $"The current workspace '{_workspaceService.CurrentWorkspace.Name}' has been saved to:\n{result}");

                        return;
                    }

                    await App.DialogService.ShowMessageAsync("Workspace Saved",
                        $"The current workspace has been renamed from '{old}' to '{name}' and saved to:{result}");

                    break;
                case ButtonId.New:
                    name = await App.DialogService.ShowTextAsync("Rename Workspace", "Enter a name for the new workspace you wish to create.");

                    if (name == null) return;

                    await _workspaceService.Create(name);

                    WorkspaceModel.ClearControls();
                    WorkspaceModel.LoadControls();

                    await _sessionService.Select(Guid.Empty);

                    break;
                case ButtonId.Load:
                    path = await App.DialogService.ShowOpenFileAsync(
                       $"Open a TitanControl workspace");

                    if (path is null)
                        return;

                    RequestSplash?.Invoke(this, true);

                    // Cheeky but is better for user experience
                    await Task.Delay(400);

                    try
                    {
                        await _workspaceService.LoadAsync(path);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "The selected workspace could not be loaded.", LoggingCategory);
                        await App.DialogService.ShowMessageAsync("Workspace error", $"The selected workspace could not be loaded\n{ex.Message}");
                        return;
                    }

                    await _sessionService.Select(Guid.Empty);

                    WorkspaceModel.ClearControls();
                    WorkspaceModel.LoadControls();

                    await TryEnableSession();

                    RequestSplash?.Invoke(this, false);
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

            CurrentPage = visible ? page : PageId.None;
             
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
                await TryEnableSession();
                return;
            }

            var name = PathHelper.GetNextFileName(
                "Untitled Workspace",
                _workspaceService.WorkspaceNames);

            await _workspaceService.Create(name);
        }

        private async Task TryEnableSession()
        {
            var session = _workspaceService.CurrentWorkspace.Options.Session;

            if (session != Guid.Empty)
                await _sessionService.Select(session);
        }
    }
}
