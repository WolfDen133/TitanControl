using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using CommunityToolkit.Mvvm.Input;
using Humanizer.Localisation;
using System.Threading.Tasks;
using Tmds.DBus.Protocol;

namespace TitanControl.Views;

public partial class TextDialogWindow : ModalDialogWindow
{
    public static readonly StyledProperty<string?> ValueProperty =
        AvaloniaProperty.Register<TextDialogWindow, string?>(nameof(Value));

    public TextDialogWindow() : base()
    {
        InitializeComponent();
    }

    public string? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    [RelayCommand]
    public void TextAccept()
    {
        Close(Value);
    }

    [RelayCommand]
    public void TextDecline()
    {
        Close(null);
    }

    public async Task<string?> ShowDialog(string title, string message)
    {
        return await ShowDialog(title, message, "Accept", "Decline");
    }

    public async Task<string?> ShowDialog(string title, string message, string acceptText, string? declineText)
    {
        IsModal = true;
        Text = message;
        Title = "TitanControl - " + title;
        Heading = title;
        AcceptText = acceptText;
        DeclineText = declineText;

        return await ShowDialog<string?>(ParentWindow);
    }
}