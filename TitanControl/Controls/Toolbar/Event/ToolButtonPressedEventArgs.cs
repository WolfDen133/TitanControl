using System;
using TitanControl.Controls.Toolbar.Buttons;

namespace TitanControl.Controls.Toolbar.Event
{
    public class ToolButtonPressedEventArgs : EventArgs
    {
        public required ToolbarButton.ButtonAction ButtonAction { get; init; }
        public required ButtonId ButtonId { get; init; }
    }
}
