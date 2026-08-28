using Avalonia;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace TitanControl.Converters
{
    internal class HaloStringToBrushConverter : IValueConverter
    {
        public object Convert(
            object? value,
            Type targetType,
            object? parameter,
            CultureInfo culture)
        {
            if (value is not string hex || string.IsNullOrWhiteSpace(hex))
            {
                return AvaloniaProperty.UnsetValue;
            }

            if (!Color.TryParse(hex, out Color color))
                return BindingOperations.DoNothing;

            // No opacity parameter — preserve alpha from the hex value.
            if (parameter is null ||
                string.IsNullOrWhiteSpace(parameter.ToString()))
            {
                return new SolidColorBrush(color);
            }

            if (!double.TryParse(
                    parameter.ToString(),
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out var opacity))
            {
                return new BindingNotification(
                    new ArgumentException("Expected opacity parameter between 0 and 1."),
                    BindingErrorType.Error);
            }

            opacity = Math.Clamp(opacity, 0, 1);

            // Override the alpha encoded in the hex.
            var alpha = (byte)Math.Round(opacity * 255);

            return new SolidColorBrush(
                Color.FromArgb(alpha, color.R, color.G, color.B));
        }

        public object ConvertBack(
            object? value,
            Type targetType,
            object? parameter,
            CultureInfo culture)
        {
            return BindingOperations.DoNothing;
        }
    }
}
