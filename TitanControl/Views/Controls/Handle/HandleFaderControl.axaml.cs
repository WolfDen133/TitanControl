using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Channels;
using System.Windows.Input;
using TitanControl.Logging;
using TitanControl.Models.Control.Handle;
using TitanControl.Views.Controls.Layout.Grid;

namespace TitanControl.Views.Controls.Handle
{
    [PseudoClasses(":pressed")]
    public class HandleFaderControl : HandleBaseControl
    {
        public static readonly StyledProperty<double> ValueProperty =
            AvaloniaProperty.Register<HandleFaderControl, double>(nameof(Value), 0d);

        public static readonly StyledProperty<double> CellHeightProperty =
            AvaloniaProperty.Register<HandleFaderControl, double>(nameof(CellHeightProperty), 0d);

        public static readonly StyledProperty<double> CellYSpanProperty =
            AvaloniaProperty.Register<HandleFaderControl, double>(nameof(CellYSpan), 0d);

        public static readonly StyledProperty<bool> ButtonEnabledProperty =
            AvaloniaProperty.Register<HandleFaderControl, bool>(nameof(ButtonEnabled), false);

        public static readonly StyledProperty<ICommand?> PressCommandProperty =
            AvaloniaProperty.Register<HandleButtonControl, ICommand?>(
                nameof(PressCommand));

        public static readonly StyledProperty<ICommand?> ReleaseCommandProperty =
            AvaloniaProperty.Register<HandleButtonControl, ICommand?>(
                nameof(ReleaseCommand));

        private readonly Transitions _releaseTransitions =
        [
            new DoubleTransition
            {
                Property = Visual.OpacityProperty,
                Duration = TimeSpan.FromMilliseconds(200),
                Easing = new CubicEaseOut()
            }
        ];

        private Border? _pressedOverlay;
        private Border? _info;
        private Border? _fader;
        private Rectangle? _progress;
        private Rectangle? _pill;

        private bool _pointerDown = false;
        private double _last = 0;

        public FaderControlMode Mode
        {
            get;
            set;
        } = FaderControlMode.Relative;

        public double Min
        {
            get;
            set;
        } = 0;

        public double Max
        {
            get;
            set;
        } = 100;
       

        public HandleFaderControl()
        {
            Classes.CollectionChanged += (_, _) => HandleButtonEnabled();
        }

        public ICommand? PressCommand
        {
            get => GetValue(PressCommandProperty);
            set => SetValue(PressCommandProperty, value);
        }

        public ICommand? ReleaseCommand
        {
            get => GetValue(ReleaseCommandProperty);
            set => SetValue(ReleaseCommandProperty, value);
        }

        public double Value
        {
            get => GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public bool ButtonEnabled
        {
            get => GetValue(ButtonEnabledProperty);
            set => SetValue(ButtonEnabledProperty, value);
        }

        public double CellHeight
        {
            get => GetValue(CellHeightProperty);
            set => SetValue(CellHeightProperty, value);
        }

        public double CellYSpan 
        { 
            get => GetValue(CellYSpanProperty); 
            set => SetValue(CellYSpanProperty, value); 
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            _pressedOverlay = e.NameScope.Find<Border>("PART_PressedOverlay");
            _info = e.NameScope.Find<Border>("PART_Info");
            _fader = e.NameScope.Find<Border>("PART_Fader");
            _progress = e.NameScope.Find<Rectangle>("PART_FaderProgress");
            _pill = e.NameScope.Find<Rectangle>("PART_FaderPill");

            UpdateFader(new Point());
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == ButtonEnabledProperty)
            {
                Classes.Set("button", (bool)change.NewValue!);
                return;
            }

            if (change.Property == CellHeightProperty 
                || change.Property == CellYSpanProperty)
                UpdateHeight();
        }

        private void UpdateHeight()
        {
            if (_info is null)
                return;

            double infoHeight = CellHeight - 1;
            _info.Height = CellYSpan > 2 ? infoHeight : Bounds.Height / 2;

            UpdateVisual();
        }

        private void HandleButtonEnabled()
        {
            if (_info is null || _pressedOverlay is null)
                return;

            if (Classes.Contains("button"))
            {
                _info.PointerPressed += InfoButton_PointerPressed;
                _info.PointerReleased += InfoButton_PointerReleased;
                return;
            }

            _info.PointerPressed -= InfoButton_PointerPressed;
            _info.PointerReleased -= InfoButton_PointerReleased;
        }

        private void InfoButton_PointerReleased(object? sender, Avalonia.Input.PointerReleasedEventArgs e)
        {
            PseudoClasses.Set(":pressed", false);

            if (_pressedOverlay is not null)
            {
                // Release fades smoothly away.
                _pressedOverlay.Transitions = _releaseTransitions;
                _pressedOverlay.Opacity = 0;
            }

            ReleaseCommand?.Execute(null);

            e.Pointer.Capture(null);

            e.Handled = true;
        }

        private void InfoButton_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            if (!IsInteractable)
                return;

            e.Pointer.Capture(this);

            if (_pressedOverlay is not null)
            {
                // Press is instant.
                _pressedOverlay.Transitions = null;
                _pressedOverlay.Opacity = 1;
            }

            PseudoClasses.Set(":pressed", true);

            PressCommand?.Execute(null);

            e.Handled = true;
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);

            _pointerDown = true;

            if (!IsInteractable)
                return;

            Log.Debug("Fader clicked");

            e.Pointer.Capture(this);

            UpdateFader(e.GetPosition(_fader));

            e.Handled = true;
        }

        protected override void OnPointerMoved(PointerEventArgs e)
        {
            base.OnPointerMoved(e);

            if (!_pointerDown)
                return;

            Log.Debug("Fader moved");

            UpdateFader(e.GetPosition(_fader));

            e.Handled = true;
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            base.OnPointerReleased(e);

            if (!_pointerDown)
                return;

            _pointerDown = false;
            _last = 0;

            Log.Debug("Fader released");

            e.Pointer.Capture(null);
            e.Handled = true;
        }

        protected override void OnSizeChanged(SizeChangedEventArgs e)
        {
            base.OnSizeChanged(e);

            UpdateHeight();
            UpdateVisual();
        }

        private void UpdateFader(Point cursor)
        {
            Value = Mode switch
            {
                FaderControlMode.Relative => CalculateRelativeValue(cursor),
                FaderControlMode.Absolute => CalculateAbsouluteValue(cursor),
                _ => 0
            };

            Log.Debug($"Fader value {Value}");

            UpdateVisual();
        }

        private double CalculateAbsouluteValue(Point cursor)
        {
            double value = 
                ((_fader!.Bounds.Height - cursor.Y) / _fader.Bounds.Height) * 100d;

            return Math.Max(Math.Min(value, Max), Min);
        }

        private double CalculateRelativeValue(Point cursor)
        {
            if (_last == 0)
            {
                _last = cursor.Y;
                return Value;
            }

            double diff = ((_last - cursor.Y) / _fader!.Bounds.Height) * 100d;
            _last = cursor.Y;

            return Math.Max(Math.Min(Value + diff, Max), Min);
        }

        private void UpdateVisual()
        {
            if (_progress is null || _fader is null)
                return;

            double valueHeight = _fader!.Bounds.Height * (Value / 100d);
            double pillHeight = (_fader!.Bounds.Height - _pill!.Bounds.Height) * (Value / 100d);

            _progress.Height = valueHeight;
            Canvas.SetBottom(_progress, 0);
            Canvas.SetBottom(_pill, pillHeight);
        }
    }
}