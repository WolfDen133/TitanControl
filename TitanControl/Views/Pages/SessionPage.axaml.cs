using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using TitanControl.Events.Control;
using TitanControl.Helper;
using TitanControl.Logging;
using TitanControl.ViewModels.Page;
using TitanControl.Views.Controls.Menu;

namespace TitanControl.Views.Pages
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

            RefreshButton.AddHandler(Button.ClickEvent, RefreshButton_Click);
        }

        private void Model_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Log.Debug($"Property Changed {e.PropertyName}");

			if (e.PropertyName == nameof(Model.RefreshEnabled))
            {
				Log.Debug($"Property Changed {e.PropertyName} {Model.RefreshEnabled}");
				HandleRefreshButtonVisual(RefreshButton.IsPointerOver, !Model.RefreshEnabled);
			}
        }

        protected override void OnLoaded(RoutedEventArgs e)
        {
            base.OnLoaded(e);

            if (Design.IsDesignMode)
                return;

			Model.PropertyChanged += Model_PropertyChanged;
			Model.StartScanner();
        }

        protected override void OnUnloaded(RoutedEventArgs e)
        {
            base.OnUnloaded(e);

            if (Design.IsDesignMode)
                return;

			Model.PropertyChanged -= Model_PropertyChanged;
			Model.StopScanner();
        }

        private void SessionOverview_OnSelect(object? sender, SessionOverviewSelectedEventArgs e)
        {
            if (Design.IsDesignMode)
                return;

            if (sender is SessionOverview)
                Model.HandleSessionSelect(sender, e);
        }

        private void RefreshButton_Click(object? sender, RoutedEventArgs e)
        {
            if (Design.IsDesignMode)
                return;

            if (Model.RefreshEnabled)
                Model.StartScanner();
            else
                Model.StopScanner();

		}

        private void IconButton_PointerEntered(object? sender, Avalonia.Input.PointerEventArgs e)
        {
            if (Design.IsDesignMode)
                return;

			HandleRefreshButtonVisual(true, !Model.RefreshEnabled);
		}

        private void IconButton_PointerExited(object? sender, Avalonia.Input.PointerEventArgs e)
        {
            HandleRefreshButtonVisual(false, !Model.RefreshEnabled);
		}

        private void HandleRefreshButtonVisual(bool pointerOver, bool refreshing = false)
        {
			RefreshButton.Text = refreshing ? "Stop" : "Refresh";

			if (pointerOver && refreshing)
            {
				RefreshIcon.IsVisible = false;
				StopIcon.IsVisible = true;
				RefreshButton.Foreground = ResourceHelper.GetThemeBrush("DangerLBrush");
			}

			if (!pointerOver && refreshing)
			{
				RefreshIcon.IsVisible = true;
				StopIcon.IsVisible = false;
				RefreshButton.Foreground = ResourceHelper.GetThemeBrush("ForegroundBrush");
			}

            if (pointerOver && !refreshing)
            {
				RefreshIcon.IsVisible = true;
				StopIcon.IsVisible = false;
				RefreshButton.Foreground = ResourceHelper.GetThemeBrush("ForegroundBrush");
			}

			if (!pointerOver && !refreshing)
			{
				RefreshIcon.IsVisible = true;
				StopIcon.IsVisible = false;
				RefreshButton.Foreground = ResourceHelper.GetThemeBrush("ForegroundBrush");
			}
		}
    }
}