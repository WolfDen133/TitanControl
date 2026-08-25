using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using System;
using System.Windows.Input;

namespace TitanControl.Views.Controls.Handle
{
    [PseudoClasses(":pressed")]
    public class HandleButtonControl : HandleBaseControl
    {
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

        public static readonly StyledProperty<string?> TitleProperty =
            AvaloniaProperty.Register<HandleButtonControl, string?>(
                nameof(Title));

        public static readonly StyledProperty<string?> SubtitleProperty =
            AvaloniaProperty.Register<HandleButtonControl, string?>(
                nameof(Subtitle));

        public static readonly StyledProperty<ICommand?> PressCommandProperty =
            AvaloniaProperty.Register<HandleButtonControl, ICommand?>(
                nameof(PressCommand));

        public static readonly StyledProperty<ICommand?> ReleaseCommandProperty =
            AvaloniaProperty.Register<HandleButtonControl, ICommand?>(
                nameof(ReleaseCommand));

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

        public string? Title
        {
            get => GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public string? Subtitle
        {
            get => GetValue(SubtitleProperty);
            set => SetValue(SubtitleProperty, value);
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            _pressedOverlay =
                e.NameScope.Find<Border>("PART_PressedOverlay");
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);

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

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            base.OnPointerReleased(e);

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
    }
}