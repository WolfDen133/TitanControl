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
    public class BoolToStateConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return ((bool?)value ?? false) ? (parameter ?? "Enabled") : "Disabled";
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return (value?.ToString() ?? "Disabled").ToLower() == (parameter?.ToString()!.ToLower() ?? "enabled") ? true : false;
        }
    }
}
