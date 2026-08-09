using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.Input;
using System.Reflection.Metadata;
using TitanControl.Services.Interface;

namespace TitanControl.Views;

public partial class ModalDialogWindow : Window
{
    public static readonly StyledProperty<string> HeadingProperty =
        AvaloniaProperty.Register<ModalDialogWindow, string>(nameof(Heading), "Content Heading");

    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<ModalDialogWindow, string?>(nameof(Text), "Content field");

    public string Heading
    {
        get => GetValue(HeadingProperty); 
        set => SetValue(HeadingProperty, value);  
    }

    public string? Text
    {
        get => GetValue(TextProperty); 
        set => SetValue(TextProperty, value);
    }

    public ModalDialogWindow()
    {
        InitializeComponent();

        DataContext = this;
    }

    [RelayCommand]
    public void AcceptCommand()
    {
        Close(true);
    }

    [RelayCommand]
    public void DeclineCommand()
    {
        Close(false);
    }
}