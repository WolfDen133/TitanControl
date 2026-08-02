using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanControl.Disk.Model.Workspace;
using TitanControl.Logging;
using TitanControl.Session;
using TitanControl.Session.Interface;
using TitanControl.Views.Page;
using TitanControl.Views.Page.Pages;
using TitanControl.Workspace;

namespace TitanControl.ViewModels
{
    public class MainWindowModel : BaseViewModel
    {
        private static string LoggingCategory = "MainWindowModel";
        
        private BasePage? _currentPage;

        public ISession<TitanControl.WebAPI.Titan> CurrentSession { get; set; } = null!;
        public WorkspaceModel CurrentWorkspace = null!;
        
        public BasePage? CurrentPage 
        {
            get => _currentPage;
            set => SetProperty(ref _currentPage, value);
        }

        public void Initialize()
        {
            LoadWorkspace();
        }

        public void LoadWorkspace()
        {
            CurrentSession = App.SessionManager.Create("Default Session", System.Net.IPAddress.Parse("127.0.0.1"), 4430, -1, false);
            CurrentWorkspace = App.WorkspaceManager.Create("Default Workspace");

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
