using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using TitanControl.Logging;
using TitanControl.ViewModels.Workspace.Handle;
using TitanControl.Views.Controls.Layout.Grid;
using TitanControl.Views.State;
using TitanControl.WebAPI.Data;

namespace TitanControl.Views.Controls.Handle
{
    [PseudoClasses(":selected", ":moving")]
    public abstract class HandleBaseControl : ContentControl, ISelectable
    {
        public static readonly StyledProperty<int> UserNumberProperty =
            AvaloniaProperty.Register<HandleBaseControl, int>(nameof(UserNumber), -1);

        public static readonly StyledProperty<string?> LegendProperty =
            AvaloniaProperty.Register<HandleBaseControl, string?>(nameof(Legend), "HandleLegend");

        public static readonly StyledProperty<Bitmap?> ImageProperty =
            AvaloniaProperty.Register<HandleBaseControl, Bitmap?>(nameof(Image));

        public static readonly StyledProperty<bool> HasImageProperty =
            AvaloniaProperty.Register<HandleBaseControl, bool>(nameof(HasImage), false);

        public static readonly StyledProperty<string> HaloProperty =
             AvaloniaProperty.Register<HandleBaseControl, string>(nameof(Halo), "#555555");

        public static readonly StyledProperty<HandleKeyProfile> KeyProfileProperty =
            AvaloniaProperty.Register<HandleBaseControl, HandleKeyProfile>(nameof(KeyProfile));

        public static readonly StyledProperty<bool> IsSelectedProperty =
            AvaloniaProperty.Register<HandleBaseControl, bool>(nameof(IsSelected));

        public static readonly StyledProperty<bool> IsMovingProperty =
            AvaloniaProperty.Register<HandleBaseControl, bool>(nameof(IsMoving));

        public static readonly StyledProperty<bool> IsInteractableProperty =
            AvaloniaProperty.Register<HandleBaseControl, bool>(nameof(IsInteractable), true);


        public static readonly AttachedProperty<int> GridXProperty =
            AvaloniaProperty.RegisterAttached<GridLayout, Control, int>(
                "GridX");

        public static readonly AttachedProperty<int> GridYProperty =
            AvaloniaProperty.RegisterAttached<GridLayout, Control, int>(
                "GridY");

        public static readonly AttachedProperty<int> GridXSpanProperty =
            AvaloniaProperty.RegisterAttached<GridLayout, Control, int>(
                "GridXSpan");

        public static readonly AttachedProperty<int> GridYSpanProperty =
            AvaloniaProperty.RegisterAttached<GridLayout, Control, int>(
                "GridYSpan");
 

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);


            if (change.Property == EditMode.IsEnabledProperty)
            {
                IsEnabled = !EditMode.GetIsEnabled(this);
                return;
            }

            if (change.Property == IsSelectedProperty)
            {
                PseudoClasses.Set(":selected", (bool)change.NewValue!);
                return;
            }

            if (change.Property == IsMovingProperty)
            {
                PseudoClasses.Set(":moving", (bool)change.NewValue!);
                return;
            }

            if (change.Property == ImageProperty)
            {
                HasImage = change.NewValue != null;
            }
        }


        public int UserNumber
        {
            get => GetValue(UserNumberProperty);
            set => SetValue(UserNumberProperty, value);
        }

        public string? Legend
        {
            get => GetValue(LegendProperty);
            set => SetValue(LegendProperty, value);
        }

        public Bitmap? Image
        {
            get => GetValue(ImageProperty);
            set => SetValue(ImageProperty, value);
        }

        public bool HasImage
        {
            get => GetValue(HasImageProperty);
            set => SetValue(HasImageProperty, value);
        }

        public string Halo
        {
            get => GetValue(HaloProperty);
            set => SetValue(HaloProperty, value);
        }

        public HandleKeyProfile KeyProfile
        {
            get => GetValue(KeyProfileProperty);
            set => SetValue(KeyProfileProperty, value);
        }

        public bool IsSelected
        {
            get => GetValue(IsSelectedProperty);
            set => SetValue(IsSelectedProperty, value);
        }

        public bool IsMoving
        {
            get => GetValue(IsMovingProperty);
            set => SetValue(IsMovingProperty, value);
        }

        public bool IsInteractable
        {
            get => GetValue(IsInteractableProperty);
            set => SetValue(IsInteractableProperty, value);
        }
    }
}
