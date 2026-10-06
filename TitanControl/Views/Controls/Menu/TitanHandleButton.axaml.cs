using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using System.Threading.Tasks;
using TitanControl.Helpers;
using TitanControl.Views.Controls.Menu;
using TitanControl.WebAPI.Data;

namespace TitanControl.Views.Controls.Menu;

[PseudoClasses(":pressed", ":has-image")]
public class TitanHandleButton : TemplatedControl
{
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

    public static readonly StyledProperty<Bitmap?> ImageSourceProperty =
        AvaloniaProperty.Register<TitanHandleButton, Bitmap?>(nameof(ImageSource), null);

    public static readonly StyledProperty<bool> IsSelectedProperty =
        AvaloniaProperty.Register<TitanHandleButton, bool>(nameof(IsSelected), false);

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

    public Bitmap? ImageSource
    {
        get => GetValue(ImageSourceProperty);
        set => SetValue(ImageSourceProperty, value);
    }

    public IBrush? BackgroundGradient
    {
        get => GetValue(BackgroundGradientProperty);
        set => SetValue(BackgroundGradientProperty, value);
    }

    public bool IsSelected
    {
        get => GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == HandleTypeProperty)
        {
            var newType = change.GetNewValue<HandleType>();

            OnHandleTypeChanged(newType);
        }

        if (change.Property == IconProperty)
        {
            var newIcon = change.GetNewValue<string?>();
            if (!string.IsNullOrEmpty(newIcon))
                _ = LoadImage();
            else
                ImageSource = null;

            PseudoClasses.Set(":has-image", !string.IsNullOrEmpty(newIcon) && ImageSource != null);
        }

        if (change.Property == IsSelectedProperty)
        {
            PseudoClasses.Set(":selected", change.GetNewValue<bool>());
        }
    }

    private async Task LoadImage()
    {
        ImageSource = await ImageHelper.LoadImageAsync(Icon!);
    }

    private void OnHandleTypeChanged(HandleType type)
    {
        Classes.Clear();
        Classes.Add(type.ToString().ToLower());
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        e.Pointer.Capture(this);

        PseudoClasses.Set(":pressed", true);

        IsSelected = !IsSelected;
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);

        e.Pointer.Capture(null);

        PseudoClasses.Set(":pressed", false);
    }
}