using System;
using TitanControl.Views.Controls.Toolbar.Button;
using TitanControl.Views.Controls.Toolbar.Buttons;

namespace TitanControl.Events.Control
{
    public class ToolButtonPressedEventArgs : EventArgs
    {
        public required ToolbarButton.ButtonAction ButtonAction { get; init; }
        public required ButtonId ButtonId { get; init; }
    }
}
