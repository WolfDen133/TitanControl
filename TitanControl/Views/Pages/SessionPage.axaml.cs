using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using TitanControl.Controls.Menu;
using TitanControl.Events.Control;
using TitanControl.ViewModels;
using TitanControl.ViewModels.Page;

namespace TitanControl.Views.Page.Pages
{
    public partial class SessionPage : UserControl
    {
        public SessionPageModel Model
        {
            get
            {
                if (DataContext is not SessionPageModel m)
                    throw new InvalidOperationException($"Could not find valid data context for {nameof(SessionPage)}.");

                return m;
            }
        }

        public SessionPage()
        {
            InitializeComponent();
        }

        protected override void OnLoaded(RoutedEventArgs e)
        {
            base.OnLoaded(e);

            Model.StartScanner();
        }

        protected override void OnUnloaded(RoutedEventArgs e)
        {
            base.OnUnloaded(e);

            Model.StopScanner();
        }

        private void SessionOverview_OnSelect(object? sender, SessionOverviewSelectedEventArgs e)
        {
            if (sender is SessionOverview)
                Model.HandleSessionSelect(sender, e);
        }

        private void Button_Click(object? sender, RoutedEventArgs e)
        {
            Model.StartScanner();
        }
    }
}