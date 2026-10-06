using Avalonia;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TitanControl.Converters
{
    public class BrushOpacityConverter : IValueConverter
    {
        public object Convert(
             object? value,
             Type targetType,
             object? parameter,
             CultureInfo culture)
        {
            if (value is not SolidColorBrush brush)
                return AvaloniaProperty.UnsetValue;

            // No opacity parameter — preserve alpha from the hex value.
            if (parameter is null || string.IsNullOrWhiteSpace(parameter.ToString()))
                return BindingOperations.DoNothing;

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

            var color = brush.Color;

            return new SolidColorBrush(Color.FromArgb(alpha, color.R, color.G, color.B));
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return BindingOperations.DoNothing;
        }
    }
}
