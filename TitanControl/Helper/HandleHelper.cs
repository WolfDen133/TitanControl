using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanControl.WebAPI.Data;

namespace TitanControl.Helper
{
    public class HandleHelper
    {
        public static string ToFormattedString(HandleType type)
        {
            return type switch
            {
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
