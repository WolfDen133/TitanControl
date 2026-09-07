using Avalonia.Interactivity;
using System;
using TitanControl.Views.Controls.Toolbar.Button;
using TitanControl.Views.Controls.Toolbar.Buttons;

namespace TitanControl.Events.Control
{
    public class ToolButtonPressedEventArgs : RoutedEventArgs
    {
        public ToolButtonPressedEventArgs(RoutedEvent revent) : base(revent)
        { }

        public required ButtonAction ButtonAction { get; init; }
        public required ButtonId ButtonId { get; init; }
    }
}
