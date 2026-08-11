using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TitanControl.Disk.Model.Workspace;
using TitanControl.Logging;
using TitanControl.Session;
using TitanControl.Session.Interface;
using TitanControl.Views.Page;
using TitanControl.Views.Page.Pages;
using TitanControl.Workspaces;

namespace TitanControl.ViewModels
{
    public class MainWindowModel : BaseViewModel
    {
        private static string LoggingCategory = "MainWindowModel";
        
        private BasePage? _currentPage;

        public Workspace CurrentWorkspace = null!;
        public ISession? CurrentSession = null!;
        
        public BasePage? CurrentPage 
        {
            get => _currentPage;
            set => SetProperty(ref _currentPage, value);
        }

        public void Initialize()
        {
            _ = LoadWorkspace();
        }

        public async Task LoadWorkspace()
        {
            await App.SessionManager.Load();

            CurrentWorkspace = App.WorkspaceManager.HasLastWorkspace() 
                ? await App.WorkspaceManager.LoadLastWorkspace()
                : App.WorkspaceManager.Create("Untitled workspace");

            App.SessionManager.Sessions.FirstOrDefault(s => s.ID == CurrentWorkspace.Options.Session)?.Enable();

            if (MainWindow.PageManager.TryGetPage(PageId.Workspace, out var page))
            {
                if (page is not WorkspacePage p)
                {
                    Log.Error("Could not find workspace page to load to.", LoggingCategory);
                    return;
                }

                p.LoadControls(CurrentWorkspace);
            }
        }
    }
}
