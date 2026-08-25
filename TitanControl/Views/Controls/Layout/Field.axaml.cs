using Avalonia;
using Avalonia.Controls;

namespace TitanControl.Views.Controls.Layout;

public class Field : ContentControl
{
    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<Field, string?>(nameof(Text), null);

    public static readonly StyledProperty<bool> RequiredProperty =
        AvaloniaProperty.Register<Field, bool>(nameof(Required));

    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public bool Required
    {
        get => GetValue(RequiredProperty);
        set => SetValue(RequiredProperty, value);
    }
}