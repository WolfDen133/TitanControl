using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace TitanControl.Controls.Common;

public class Field : ContentControl
{
    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<Field, string?>(nameof(Text), "Field");

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