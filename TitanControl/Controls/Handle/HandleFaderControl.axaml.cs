using Avalonia;
using Avalonia.Controls.Primitives;

namespace TitanControl.Controls.Handle
{
    public class HandleFaderControl : HandleBaseControl
    {
        public static readonly StyledProperty<double> ValueProperty =
            AvaloniaProperty.Register<HandleFaderControl, double>(nameof(Value));

        public static readonly StyledProperty<double> MinimumProperty =
            AvaloniaProperty.Register<HandleFaderControl, double>(
                nameof(Minimum),
                0);

        public static readonly StyledProperty<double> MaximumProperty =
            AvaloniaProperty.Register<HandleFaderControl, double>(
                nameof(Maximum),
                1);

        public double Value
        {
            get => GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public double Minimum
        {
            get => GetValue(MinimumProperty);
            set => SetValue(MinimumProperty, value);
        }

        public double Maximum
        {
            get => GetValue(MaximumProperty);
            set => SetValue(MaximumProperty, value);
        }
    }
}