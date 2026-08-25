using Avalonia.Interactivity;

namespace TitanControl.Events
{
    public sealed class ToggledEventArgs : RoutedEventArgs
    {
        public ToggledEventArgs(
            RoutedEvent routedEvent,
            bool value)
            : base(routedEvent)
        {
            Value = value;
        }

        public bool Value { get; }
    }
}
