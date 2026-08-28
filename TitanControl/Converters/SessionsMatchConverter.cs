using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Globalization;
using TitanControl.Services.Session;

namespace TitanControl.Converters
{
    public class SessionsMatchConverter : IValueConverter
    {
        public bool Inverted { get; set; } = false;

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var result = ((Guid?)value) == ((Guid?)parameter) ? true : false;

            return Inverted ? !result : result;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return BindingOperations.DoNothing;
        }
    }
}
