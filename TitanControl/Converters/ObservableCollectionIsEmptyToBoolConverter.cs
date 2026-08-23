using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using TitanControl.Services.Session;

namespace TitanControl.Converters
{
    public class ObservableCollectionIsEmptyToBoolConverter : IValueConverter
    {
        public bool Inverted { get; set; } = false;

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            bool result = ((ObservableCollection<TitanSession>?)value)!.Count == 0;

            return Inverted ? !result : result;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return BindingOperations.DoNothing;
        }
    }
}
