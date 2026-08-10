using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.ComponentModel;
using System.Diagnostics.Tracing;
using TitanControl.Events.Control;
using TitanControl.Helper;
using TitanControl.Session;
using TitanControl.Session.Utils;
using static TitanControl.Controls.Toolbar.Buttons.ToolbarButton;

namespace TitanControl.Controls.Menu
{
    public partial class SessionOverview : UserControl
    {
        public static readonly StyledProperty<string> SessionNameProperty =
            AvaloniaProperty.Register<SessionOverview, string>(nameof(SessionName), "Session name");

        public static readonly StyledProperty<SessionConnectionState> SessionStatusProperty =
            AvaloniaProperty.Register<SessionOverview, SessionConnectionState>(nameof(SessionStatus), SessionConnectionState.Available);

        public static readonly StyledProperty<string> IpAddressProperty =
            AvaloniaProperty.Register<SessionOverview, string>(nameof(IpAddress), "192.168.1.1");

        public static readonly StyledProperty<string> PortProperty =
            AvaloniaProperty.Register<SessionOverview, string>(nameof(Port), "4430");

        public static readonly StyledProperty<string?> PortInteractiveProperty =
            AvaloniaProperty.Register<SessionOverview, string?>(nameof(PortInteractive), "4431");

        public static readonly StyledProperty<string> HostProperty =
            AvaloniaProperty.Register<SessionOverview, string>(nameof(Host), "TITAN-MACHINE 1");

        public static readonly StyledProperty<bool> IsSelectedProperty =
            AvaloniaProperty.Register<SessionOverview, bool>(nameof(IsSelected), false);

        public static readonly StyledProperty<Guid> IdProperty =
            AvaloniaProperty.Register<SessionOverview, Guid>(nameof(Id), Guid.Empty);

        public static readonly StyledProperty<IRelayCommand?> CommandProperty =
            AvaloniaProperty.Register<SessionOverview, IRelayCommand?>(nameof(Command));

        public static readonly StyledProperty<object?> CommandParameterProperty =
           AvaloniaProperty.Register<SessionOverview, object?>(nameof(CommandParameter));


        public string SessionName 
        {
            get => GetValue(SessionNameProperty); 
            set => SetValue(SessionNameProperty, value); 
        }

        public SessionConnectionState SessionStatus
        {
            get => GetValue(SessionStatusProperty);
            set => SetValue(SessionStatusProperty, value);
        }
        
        public string IpAddress
        {
            get => GetValue(IpAddressProperty);
            set => SetValue(IpAddressProperty, value);
        }

        public string Port
        {
            get => GetValue(PortProperty);
            set => SetValue(PortProperty, value);
        }

        public string? PortInteractive
        {
            get => GetValue(PortInteractiveProperty);
            set => SetValue(PortInteractiveProperty, value);
        }

        public string Host
        {
            get => GetValue(HostProperty);
            set => SetValue(HostProperty, value);
        }

        public IRelayCommand? Command
        {
            get => GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        public object? CommandParameter
        {
            get => GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }

        public bool IsSelected
        {
            get => GetValue(IsSelectedProperty);
            set => SetValue(IsSelectedProperty, value);
        }

        public Guid Id 
        { 
            get => GetValue(IdProperty);
            set => SetValue(IdProperty, value);
        }

        public event EventHandler<SessionOverviewSelectedEventArgs>? OnSelect;

        public string PortsString => PortInteractive is not null ? $"{Port} / {PortInteractive}" : Port;

        private bool mouseDown = false;

        public SessionOverview()
        {
            InitializeComponent();
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == IsSelectedProperty)
            {
                Select(change.GetNewValue<bool>());
            }
        }

        protected override void OnPointerEntered(PointerEventArgs e)
        {
            base.OnPointerEntered(e);

            if (!IsSelected)
                Background = ResourceHelper.GetThemeBrush("BackgroundLLBrush");
        }

        protected override void OnPointerExited(PointerEventArgs e)
        {
            base.OnPointerExited(e);

            if (!IsSelected)
            {
                Background = ResourceHelper.GetThemeBrush("BackgroundLBrush");
            }
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);

            mouseDown = true;
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            base.OnPointerReleased(e);

            if (mouseDown)
            {
                OnSelect?.Invoke(
                    this,
                    new SessionOverviewSelectedEventArgs(
                        Id,
                        true));
            }

            mouseDown = false;
        }

        private void Select(bool isSelected = true)
        {
            BorderThickness = isSelected ? new Thickness(2) : new Thickness(1);
            BorderBrush = isSelected ? ResourceHelper.GetThemeBrush("SelectionFocusBrush") : ResourceHelper.GetThemeBrush("BorderSubtleBrush");
            Background = isSelected ? ResourceHelper.GetThemeBrush("BackgroundLLBrush") : ResourceHelper.GetThemeBrush("BackgroundLBrush");
        }
    }
}