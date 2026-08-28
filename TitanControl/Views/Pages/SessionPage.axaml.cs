using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using System;
using System.Reflection.PortableExecutable;
using System.Threading.Tasks;
using System.Timers;
using TitanControl.Events.Control;
using TitanControl.Helper;
using TitanControl.Logging;
using TitanControl.ViewModels.Page;
using TitanControl.Views.Controls.Menu;

namespace TitanControl.Views.Pages
{
    public partial class SessionPage : BasePage
    {
        private bool _headerHovered;
        private int _popupAnimation;

        private TranslateTransform ConsoleDetailsTransform =>
            (TranslateTransform)ConsoleDetailsContent.RenderTransform!;

        private Timer _popupTimer = new();

        public SessionPageModel Model
        {
            get
            {
                if (DataContext is not SessionPageModel m)
                    throw new InvalidOperationException($"Could not find valid data context for {nameof(SessionPage)}.");

                return m;
            }
        }

        public override PageId Id => PageId.Session;
        public override Dock Dock => Dock.Top;

        public SessionPage()
        {
            InitializeComponent();

            RefreshButton.AddHandler(Button.ClickEvent, RefreshButton_Click);

            _popupTimer.Interval = 3000;
            _popupTimer.Elapsed += PopupTimer_Elapsed;
        }

        private void PopupTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(TryHideConsoleDetails);
        }

        protected override void OnLoaded(RoutedEventArgs e)
        {
            base.OnLoaded(e);

            if (Design.IsDesignMode)
                return;
            
            Dispatcher.Post(() =>
            {
                Model.PropertyChanged += Model_PropertyChanged;
            }, DispatcherPriority.Loaded);

            ConnectedSession.PointerEntered += ConnectedSession_PointerEntered;
            ConnectedSession.PointerExited += ConnectedSession_PointerExited;
        }

        protected override void OnUnloaded(RoutedEventArgs e)
        {
            base.OnUnloaded(e);

            if (Design.IsDesignMode)
                return;

			Model.PropertyChanged -= Model_PropertyChanged;
            ConnectedSession.PointerEntered -= ConnectedSession_PointerEntered;
            ConnectedSession.PointerExited -= ConnectedSession_PointerExited;
        }

        private void Model_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Model.RefreshEnabled))
                Dispatcher.Invoke(() => 
                    HandleRefreshButtonVisual(RefreshButton.IsPointerOver, !Model.RefreshEnabled));

            else if (e.PropertyName == nameof(Model.Device))
                Dispatcher.Invoke(() =>
                {
                    if (Model.Device?.ComputerName != null)
                    {
                        ShowConsoleDetails();
                        _popupTimer.Stop();
                        _popupTimer.Interval = 3000;
                        _popupTimer.Start();
                    }
                    else
                        HideConsoleDetails();
                });
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

        private void ConnectedSession_PointerEntered(object? sender, PointerEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                if (Model.EnabledSession?.State != Services.Session.SessionConnectionState.Connected)
                    return;

                _headerHovered = true;

                ShowConsoleDetails();
            });
        }

        private void ConnectedSession_PointerExited(object? sender, PointerEventArgs e)
        {
            _headerHovered = false;

            HideConsoleDetails();
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

        private async void ShowConsoleDetails()
        {
            ++_popupAnimation;

            // Cancel a closing animation if we're already open.
            if (ConsoleDetails.IsOpen)
            {
                ConsoleDetailsTransform.Y = 0;
                ConsoleDetailsContent.Opacity = 1;
                return;
            }

            ConsoleDetails.IsOpen = true;

            // Allow the popup to be created and measured.
            await Dispatcher.UIThread.InvokeAsync(
                () => { },
                DispatcherPriority.Loaded);

            double height = ConsoleDetailsContent.Bounds.Height;

            if (height <= 0)
                height = ConsoleDetailsContent.DesiredSize.Height;

            ConsoleDetailsClip.Width = ConnectedSession.Bounds.Width;

            // For Placement="Top", the popup's final position is above
            // ConnectedSession. Start the content translated DOWN so it is
            // hidden behind the ConnectedSession.
            var transitions = ConsoleDetailsTransform.Transitions;

            ConsoleDetailsTransform.Transitions = null;

            ConsoleDetailsTransform.Y = height;
            ConsoleDetailsContent.Opacity = 0;

            ConsoleDetailsTransform.Transitions = transitions;

            // Allow the initial transform to be rendered before starting
            // the transition.
            Dispatcher.UIThread.Post(() =>
            {
                ConsoleDetailsTransform.Y = 0;
                ConsoleDetailsContent.Opacity = 1;
            }, DispatcherPriority.Render);
        }

        private void TryHideConsoleDetails()
        {
            if (_headerHovered)
                return;

            HideConsoleDetails();
        }

        private async void HideConsoleDetails()
        {
            if (!ConsoleDetails.IsOpen)
                return;

            int animation = ++_popupAnimation;

            double height = ConsoleDetailsContent.Bounds.Height;

            if (height <= 0)
                height = ConsoleDetailsContent.DesiredSize.Height;

            // Placement="Top":
            // move DOWN towards ConnectedSession when closing.
            ConsoleDetailsTransform.Y = height;
            ConsoleDetailsContent.Opacity = 0;

            await Task.Delay(200);

            // Another open/close operation happened while we were waiting.
            if (animation != _popupAnimation)
                return;

            if (_headerHovered)
                return;

            ConsoleDetails.IsOpen = false;
        }

        private void HandleRefreshButtonVisual(bool pointerOver, bool refreshing = false)
        {
			RefreshButton.Text = refreshing ? "Stop" : "Refresh";
            StopIcon.IsVisible = pointerOver && refreshing;
            RefreshIcon.IsVisible = !(pointerOver && refreshing);
            RefreshButton.Foreground = pointerOver && refreshing 
                ? ResourceHelper.GetThemeBrush("DangerLBrush") 
                : ResourceHelper.GetThemeBrush("ForegroundBrush");
		}
    }
}