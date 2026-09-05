using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanControl.WebAPI;
using TitanControl.WebAPI.Data;
using static System.Runtime.InteropServices.JavaScript.JSType;
using TitanControl.WebAPI.Data.Conversion;
using Avalonia.Data;

namespace TitanControl.Converters
{

    public class HandleTypeToStringConverter : IValueConverter
    {
        public object Convert(
            object? value,
            Type targetType,
            object? parameter,
            CultureInfo culture)
        {

            return ToFormattedString((HandleType)value!);
        }

        public object ConvertBack(
            object? value,
            Type targetType,
            object? parameter,
            CultureInfo culture)
        {
            return BindingOperations.DoNothing;
        }

        public static string ToFormattedString(HandleType type)
        {
            return type switch
            {
                HandleType.None => "All",
                HandleType.Fixture => "Fixture",
                HandleType.Group => "Group",
                HandleType.Cue => "Cue",
                HandleType.CueList => "Cue List",
                HandleType.Chase => "Chase",
                HandleType.Track => "Track",
                HandleType.Palette => "Pallet",
                HandleType.Macro => "Macro",
                HandleType.Master => "Master",
                HandleType.Scene => "Scene",
                HandleType.Rate => "Rate",
                HandleType.PlaybackGroup => "Playback Group",
                _ => "Unknown"

            };
        }
    }
    
}
