using Avalonia.Data;
using Avalonia.Data.Converters;
using ShimSkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanControl.Session;

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
