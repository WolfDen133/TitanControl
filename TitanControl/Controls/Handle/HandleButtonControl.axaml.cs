using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using System.Windows.Input;

namespace TitanControl.Controls.Handle
{
    [PseudoClasses(":pressed")]
    public class HandleButtonControl : HandleBaseControl
    {
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

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);

            e.Pointer.Capture(this);

            PseudoClasses.Set(":pressed", true);

            PressCommand?.Execute(null);
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            base.OnPointerReleased(e);

            PseudoClasses.Set(":pressed", false);

            ReleaseCommand?.Execute(null);

            e.Pointer.Capture(null);
        }
    }
}