using Avalonia;
using Avalonia.Controls;

namespace TitanControl.Views.Controls.Layout;

public class Section : ContentControl
{
    public static readonly StyledProperty<object?> HeaderContentProperty =
        AvaloniaProperty.Register<Section, object?>(nameof(HeaderContent));

    public static readonly StyledProperty<object?> FooterContentProperty =
        AvaloniaProperty.Register<Section, object?>(nameof(HeaderContent));

    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<Section, string?>(nameof(Title), "Section title");

    public static readonly StyledProperty<string?> SubtitleProperty =
        AvaloniaProperty.Register<Section, string?>(nameof(Subtitle), "Section subtitle");

    public static readonly StyledProperty<object?> IconProperty =
        AvaloniaProperty.Register<Section, object?>(nameof(Icon), null);

    public static readonly StyledProperty<bool> IsDisabledProperty =
    AvaloniaProperty.Register<Section, bool>(
        nameof(IsDisabled),
        defaultValue: false);

    public static readonly StyledProperty<double> DisabledOpacityProperty =
        AvaloniaProperty.Register<Section, double>(
            nameof(DisabledOpacity),
            defaultValue: 0.0);

    public bool IsDisabled
    {
        get => GetValue(IsDisabledProperty);
        set => SetValue(IsDisabledProperty, value);
    }

    public double DisabledOpacity
    {
        get => GetValue(DisabledOpacityProperty);
        set => SetValue(DisabledOpacityProperty, value);
    }

    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public object? HeaderContent
    {
        get => GetValue(HeaderContentProperty);
        set => SetValue(HeaderContentProperty, value);
    }

    public object? FooterContent
    {
        get => GetValue(FooterContentProperty);
        set => SetValue(FooterContentProperty, value);
    }

    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string? Subtitle
    {
        get => GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }

    static Section()
    {
        IsDisabledProperty.Changed.AddClassHandler<Section>((section, e) =>
        {
            section.DisabledOpacity = (bool)e.NewValue! ? 0.6 : 0.0;
        });
    }
}