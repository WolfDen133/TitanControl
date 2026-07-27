using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using TitanControl.Controls.Toolbar;

namespace TitanControl.Controls.Toolbar.Buttons;

public partial class ToolbarButton : UserControl
{
    public required Toolstrip Toolstrip;

    public int ID { get; set; }
    public int[]? Children { get; set; } = null;
    public string Text
    {
        set => TextLabel.Content = value;
        get => (string?)TextLabel.Content ?? "Button";
    }

    public string Description { get; set; } = "This is the description of a mouse button";
    public bool IsMouseDown = false;

    public bool Toggle = false;
    private bool toggled = false;

    public event EventHandler<ButtonAction>? OnClick;

    public void SetSize(int size)
    {
        Width = Height = size;
    }

    public ToolbarButton()
    {
        InitializeComponent();

        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
        BorderBrush = FindResource("BorderBrush");
        Background = FindResource("BackgroundDBrush");

        InvalidateVisual();
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        IsMouseDown = true;

        ButtonBorder.BorderBrush = FindResource("ForegroundBrush");
        Background = FindResource("AccentBrushDark");
        TextLabel.Foreground = Brushes.White;
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (!IsMouseDown) return;

        if (Toggle) InternalToggle();
        else
        {
            IsMouseDown = false;

            ClickAction(ButtonAction.Click);
            OnClick?.Invoke(this, ButtonAction.Click);

            ButtonBorder.BorderBrush = FindResource("BorderBrush");
            Background = FindResource("BackgroundDBrush");
            TextLabel.Foreground = FindResource("ForegroundBrush");
        }
    }

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);
        
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        if (IsMouseDown && !Toggle)
        {
            IsMouseDown = false;
            ButtonBorder.BorderBrush = FindResource("BorderBrush");
            Background = FindResource("BackgroundDBrush");
            TextLabel.Foreground = FindResource("ForegroundBrush");
        }
    }

    private void InternalToggle()
    {
        if (toggled)
        {
            ReleaseToggle();
            return;
        }

        toggled = true;
        ClickAction(ButtonAction.ToggleDown);
        ButtonBorder.BorderBrush = FindResource("ForegroundBrush");
        Background = FindResource("AccentBrush");
        TextLabel.Foreground = Brushes.White;
    }

    public void ReleaseToggle(bool soft = false)
    {
        if (Toggle && toggled)
        {
            if (!soft) ClickAction(ButtonAction.ToggleUp);
            toggled = false;
        }

        ButtonBorder.BorderBrush = FindResource("BorderBrush");
        Background = FindResource("BackgroundDBrush");
        TextLabel.Foreground = FindResource("ForegroundBrush");
    }

    public Bitmap ToImage(string icon)
    {
        var uri = new Uri($"avares://TitanControl/Assets/Icons/{icon}");

        using var stream = AssetLoader.Open(uri);
        var bitmap = new Bitmap(stream);

        return bitmap;
    }

    protected virtual void LoadImage()
    {
        ButtonImage.Source = ToImage("knob.png");
    }



    protected virtual void ClickAction(ButtonAction action)
    { 
    
    }

    private IBrush? FindResource(string key)
    {
        if (this.TryFindResource(key, this.ActualThemeVariant, out var found))
        {
            return (IBrush?)found;
        }

        return Brushes.Black;
    }

    public enum ButtonAction
    {
        Click,
        ToggleDown,
        ToggleUp,
    }

    private void OnInitialised(object sender, EventArgs e)
    {
        LoadImage();
    }
}