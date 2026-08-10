using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TitanControl.Controls.Menu;
using TitanControl.Events.Control;
using TitanControl.Logging;
using TitanControl.ViewModels;

namespace TitanControl.Views.Page.Pages
{
    public partial class SessionPage : BasePage
    {

        public Dictionary<Guid, SessionOverview> sessionControls = new();

        public override PageId Id => PageId.Session;

        public SessionPage()
        {
            InitializeComponent();

            DataContext = new SessionPageModel();
        }

        protected override void OnLoaded(RoutedEventArgs e)
        {
            base.OnLoaded(e);
        }

        public override void OnRegister()
        {
            if (DataContext is SessionPageModel model)
            {
                model.Initialize();
            }
        }

        public override void OnShow()
        {
            if (DataContext is SessionPageModel model)
            {
                _ = model.StartScanner();
            }
        }

        public override void OnHide()
        {
            if (DataContext is SessionPageModel model)
            {
                model.StopScanner();
                _ = model.Clear();
            }
        }

        private void SessionOverview_OnSelect(object? sender, SessionOverviewSelectedEventArgs e)
        {
            if (sender is SessionOverview && DataContext is SessionPageModel model) 
            {
                model.HandleSessionSelect(sender, e);
            }
        }

        private void Button_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is SessionPageModel model)
            {
                model?.StartScanner();
            }
        }
    }
}