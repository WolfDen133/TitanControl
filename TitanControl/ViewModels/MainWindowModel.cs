using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TitanControl.Logging;
using TitanControl.Services.Session;
using TitanControl.Services.Workspace;
using TitanControl.ViewModels.Page;
using TitanControl.Views.Page.Pages;

namespace TitanControl.ViewModels
{
    public class MainWindowModel : BaseViewModel
    {
        private static string LoggingCategory = "MainWindowModel";

        private IWorkspaceService _workspaceService;
        private ISessionService _sessionService;

        private Dictionary<PageId, IPage> _pages = new();
        
        private IPage? _currentPage;
        
        public IPage? CurrentPage 
        {
            get => _currentPage;
            set => SetProperty(ref _currentPage, value);
        }

        public ISession? CurrentSession => _sessionService.CurrentSession;

        public MainWindowModel(IWorkspaceService workspaceService, ISessionService sessionService)
        {
            _workspaceService = workspaceService;
            _sessionService = sessionService;

            RegisterPageModels();
        }

        public void RegisterPageModels()
        {
            _pages.Add(PageId.Workspace, new WorkspacePageModel());
            _pages.Add(PageId.Session, new SessionPageModel(_sessionService, _workspaceService));
        }

        public void Initialize()
        {
            _ = LoadWorkspace();
        }

        public async Task LoadWorkspace()
        {
            
        }
    }
}
