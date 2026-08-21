using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanControl.Controls.Toolbar.Buttons;

namespace TitanControl.Controls.Toolbar.Event
{
    public class ToolButtonPressedEventArgs : EventArgs
    { 
        public required ToolbarButton.ButtonAction ButtonAction { get; init; }
        public required ButtonId ButtonId { get; init; }
    }
}
