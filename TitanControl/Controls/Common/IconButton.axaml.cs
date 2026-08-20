using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Svg.Skia;
using System.ComponentModel;

namespace TitanControl.Controls.Common;

public class IconButton : Button, INotifyPropertyChanged
{
    public static readonly StyledProperty<object?> IconProperty =
        AvaloniaProperty.Register<IconButton, object?>(nameof(Icon), null);

    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<IconButton, string?>(nameof(Text), "Button");

    public static readonly StyledProperty<bool> TextVisibleProperty =
        AvaloniaProperty.Register<IconButton, bool>(nameof(TextVisible), true);


    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public bool TextVisible
    {
        get => GetValue(TextVisibleProperty);
        set => SetValue(TextVisibleProperty, value);
    }
}