using Avalonia;
using Avalonia.Controls;

namespace TitanControl.Views.State
{
    public static class EditMode
    {
        public static readonly AttachedProperty<bool> IsEnabledProperty =
            AvaloniaProperty.RegisterAttached<Control, Control, bool>(
                "IsEnabled",
                defaultValue: false,
                inherits: true);

        public static void SetIsEnabled(AvaloniaObject element, bool value) =>
            element.SetValue(IsEnabledProperty, value);

        public static bool GetIsEnabled(AvaloniaObject element) =>
            element.GetValue(IsEnabledProperty);
    }
}
