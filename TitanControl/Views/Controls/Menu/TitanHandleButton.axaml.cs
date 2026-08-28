using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using TitanControl.Views.Controls.Menu;
using TitanControl.WebAPI.Data;

namespace TitanControl;

[PseudoClasses(":click", ":hover")]
public class TitanHandleButton : TemplatedControl
{
    private bool _mouseDown = false;

    public static readonly StyledProperty<int> UserNumberProperty =
     AvaloniaProperty.Register<TitanHandleButton, int>(nameof(UserNumber), -1);

    public static readonly StyledProperty<string> LegendProperty =
       AvaloniaProperty.Register<TitanHandleButton, string>(nameof(Legend), "Titan Handle");

    public static readonly StyledProperty<string?> HaloProperty =
       AvaloniaProperty.Register<TitanHandleButton, string?>(nameof(Halo), "#343B44");

    public static readonly StyledProperty<HandleType> HandleTypeProperty =
       AvaloniaProperty.Register<TitanHandleButton, HandleType>(nameof(HandleType), HandleType.None);

    public static readonly StyledProperty<string?> IconProperty =
       AvaloniaProperty.Register<TitanHandleButton, string?>(nameof(Icon), null);

    public static readonly StyledProperty<IBrush?> BackgroundGradientProperty =
        AvaloniaProperty.Register<TitanHandleButton, IBrush?>(nameof(BackgroundGradient), null);

    public int UserNumber
    {
        get => GetValue(UserNumberProperty);
        set => SetValue(UserNumberProperty, value);
    }

    public string Legend
    {
        get => GetValue(LegendProperty);
        set => SetValue(LegendProperty, value);
    }

    public string? Halo
    {
        get => GetValue(HaloProperty);
        set => SetValue(HaloProperty, value);
    }

    public HandleType HandleType
    {
        get => GetValue(HandleTypeProperty);
        set => SetValue(HandleTypeProperty, value);
    }

    public string? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public IBrush? BackgroundGradient
    {
        get => GetValue(BackgroundGradientProperty);
        set => SetValue(BackgroundGradientProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == HandleTypeProperty)
        {
            var newType = change.GetNewValue<HandleType>();

            OnHandleTypeChanged(newType);
        }
    }

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        PseudoClasses.Set(":hover", true);

        base.OnPointerEntered(e);
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        PseudoClasses.Set(":hover", false);

        base.OnPointerExited(e);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        e.Pointer.Capture(this);
        _mouseDown = true;

        PseudoClasses.Set(":click", true);

        base.OnPointerPressed(e);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        if (_mouseDown)
            _mouseDown = false;

        e.Pointer.Capture(null);
        PseudoClasses.Set(":click", false);

        base.OnPointerReleased(e);
    }

    private void OnHandleTypeChanged(HandleType type)
    {
        Classes.Clear();
        Classes.Add(type.ToString().ToLower());
    }
}