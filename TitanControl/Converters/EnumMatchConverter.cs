using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TitanControl.Converters
{
    public class EnumMatchConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            => value?.ToString() == parameter?.ToString();

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => ((bool?)value ?? false) ? System.Enum.Parse(targetType, parameter?.ToString() ?? string.Empty) : BindingOperations.DoNothing;
    }
}
