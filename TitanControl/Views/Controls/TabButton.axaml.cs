using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.Input;

namespace TitanControl.Views.Controls;

public partial class TabButton : UserControl
{
    public static readonly StyledProperty<string?> IconProperty =
       AvaloniaProperty.Register<TabButton, string?>(nameof(Icon), null);

    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<TabButton, string?>(nameof(Text), "Button");

    public static readonly StyledProperty<bool> TextVisibleProperty =
        AvaloniaProperty.Register<TabButton, bool>(nameof(TextVisible), true);

    public static readonly StyledProperty<object?> CommandParameterProperty =
        AvaloniaProperty.Register<TabButton, object?>(nameof(CommandParameter), null);

    public static readonly StyledProperty<IRelayCommand?> CommandProperty =
      AvaloniaProperty.Register<TabButton, IRelayCommand?>(nameof(Command), null);

    public static readonly StyledProperty<string?> RightContentProperty =
        AvaloniaProperty.Register<TabButton, string?>(nameof(RightContent), null);


    public string? Icon
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

    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    public IRelayCommand? Command
    {
        get => GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public string? RightContent
    {
        get => GetValue(RightContentProperty);
        set => SetValue(RightContentProperty, value);
    }

    public TabButton()
    {
        InitializeComponent();
    }
}