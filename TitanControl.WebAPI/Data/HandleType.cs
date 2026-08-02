using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TitanControl.WebAPI.Data
{
    public enum HandleType
    {
        None = -1,
        Fixture = 0,
        Group = 1,
        Cue = 2,
        CueList = 3,
        Chase = 4,
        Track = 5,
        Palette = 6,
        Macro = 7,
        Master = 8,
        Scene = 9,
        Rate = 10,
        PlaybackGroup = 11,
    }
}
