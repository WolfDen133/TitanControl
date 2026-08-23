using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace TitanControl.Converters
{
    public class ObjectIsNullConverter : IValueConverter
    {
        public bool Inverted { get; set; } = false;

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            bool result = value is null;

            return Inverted ? !result : result;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return BindingOperations.DoNothing;
        }
    }
}
