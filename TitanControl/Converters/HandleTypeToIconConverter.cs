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

namespace TitanControl.Converters
{

    public class HandleTypeToIconConverter : IValueConverter
    {
        public object Convert(
            object? value,
            Type targetType,
            object? parameter,
            CultureInfo culture)
        {
            
            return "/Assets/Icons/" + value switch
            {
                HandleType.Cue => "play",
                HandleType.Fixture => "light",
                HandleType.Group => "group",
                HandleType.CueList => "playlist",
                HandleType.Chase => "reorder",
                HandleType.Track => "left-align",
                HandleType.Palette => "palette",
                HandleType.Macro => "code",
                HandleType.Master => "crown",
                HandleType.Scene => "shuffle",
                HandleType.Rate => "speed",
                HandleType.PlaybackGroup => "group",
                _ => "question-circle"
            } + ".svg";
        }

        public object ConvertBack(
            object? value,
            Type targetType,
            object? parameter,
            CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
    
}
