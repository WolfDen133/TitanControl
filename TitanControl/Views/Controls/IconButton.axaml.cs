using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.Input;
using System;
using TitanControl.Views.Controls;

namespace TitanControl.Views.Controls;

public partial class IconButton : UserControl
{
    public static readonly StyledProperty<object?> IconProperty =
       AvaloniaProperty.Register<IconButton, object?>(nameof(Icon), null);

    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<IconButton, string?>(nameof(Text), "Button");

    public static readonly StyledProperty<bool> TextVisibleProperty =
        AvaloniaProperty.Register<IconButton, bool>(nameof(TextVisible), true);

    public static readonly StyledProperty<object?> CommandParameterProperty =
        AvaloniaProperty.Register<IconButton, object?>(nameof(CommandParameter), null);

    public static readonly StyledProperty<IRelayCommand?> CommandProperty =
      AvaloniaProperty.Register<IconButton, IRelayCommand?>(nameof(Command), null);

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

    public IconButton()
    {
        InitializeComponent();
    }
}