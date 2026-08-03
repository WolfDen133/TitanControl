using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using System;
using System.Collections.Generic;
using System.Linq;
using TitanControl.Controls.Menu;
using TitanControl.Logging;
using TitanControl.ViewModels;

namespace TitanControl.Views.Page.Pages
{
    public partial class SessionPage : BasePage
    {
        public Dictionary<Guid, SessionOverview> sessionControls = new();

        public SessionPage()
        {
            InitializeComponent();

            Id = PageId.Session;
            DataContext = new SessionPageModel();
        }

        protected override void OnLoaded(RoutedEventArgs e)
        {
            base.OnLoaded(e);

            if (DataContext is SessionPageModel model)
            {

            }
        }

        public override void OnRegister()
        {
            base.OnRegister();

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
                _ = model.UpdateSessions();
            }
        }

        public override void OnHide()
        {
            if (DataContext is SessionPageModel model)
            {
                model.StopScanner();
            }
        }

        // TODO: Fix
        private void SessionOverview_OnSelect(object? sender, bool e)
        {
            if (sender is SessionOverview sessionOverview)
            {
                Log.Debug("Is Session");
                if (DataContext is SessionPageModel model)
                {
                    Log.Debug("Is Model");
                    foreach (var session in model.ScanResults)
                    {
                        if (session.Id == sessionOverview.Id)
                            continue;

                        Log.Debug(session.Id.ToString() + " " + sessionOverview.Id.ToString());
                        session.IsSelected = false;
                    }
                }
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