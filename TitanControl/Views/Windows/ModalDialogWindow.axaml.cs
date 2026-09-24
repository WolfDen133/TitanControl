using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using CommunityToolkit.Mvvm.Input;
using Humanizer.Localisation;
using System.Threading.Tasks;

namespace TitanControl.Views;

public partial class ModalDialogWindow : Window
{
    public static readonly StyledProperty<string> HeadingProperty =
        AvaloniaProperty.Register<ModalDialogWindow, string>(nameof(Heading), "Content Heading");

    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<ModalDialogWindow, string?>(nameof(Text), "Content field");

    public static readonly StyledProperty<string> AcceptTextProperty =
        AvaloniaProperty.Register<ModalDialogWindow, string>(nameof(AcceptText), "Accept");

    public static readonly StyledProperty<string?> DeclineTextProperty =
        AvaloniaProperty.Register<ModalDialogWindow, string?>(nameof(DeclineText), "Decline");

    public static readonly StyledProperty<bool> IsModalProperty =
        AvaloniaProperty.Register<ModalDialogWindow, bool>(nameof(IsModal), true);

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

    public string AcceptText
    {
        get => GetValue(AcceptTextProperty);
        set => SetValue(AcceptTextProperty, value);
    }

    public string? DeclineText
    {
        get => GetValue(DeclineTextProperty);
        set => SetValue(DeclineTextProperty, value);
    }

    public bool IsModal
    {
        get => GetValue(IsModalProperty);
        set => SetValue(IsModalProperty, value);
    }

    public required Window ParentWindow;

    public ModalDialogWindow()
    {
        InitializeComponent();

        DataContext = this;
    }

    [RelayCommand]
    public void Accept()
    {
        if (IsModal)
            Close(true);
        else
            Close();
    }

    [RelayCommand]
    public void Decline()
    {
        if (IsModal)
            Close(false);
        else
            Close();
    }

    public async Task ShowMessage(string title, string message)
    {
        await ShowMessage(title, message, "Accept");
    }

    public async Task ShowMessage(string title, string message, string acceptText)
    {
        IsModal = false;
        Text = message;
        Title = "TitanControl - " + title;
        Heading = title;
        AcceptText = acceptText;
        DeclineText = null;

        await ShowDialog(ParentWindow);
    }

    public async Task<TResult?> ShowDialog<TResult>(string title, string message, string acceptText, string? declineText, bool isModal)
    {
        IsModal = isModal;
        Text = message;
        Title = "TitanControl - " + title;
        Heading = title;
        AcceptText = acceptText;
        DeclineText = declineText;

        return await ShowDialog<TResult>(ParentWindow);
    }
}