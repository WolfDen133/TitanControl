using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TitanControl.Views.Controls.Layout;
using TitanControl.Views.Controls.Toolbar.Button;

namespace TitanControl.Views.Controls.Toolbar.Buttons
{
    [PseudoClasses(":hover", ":clicked", ":toggled", ":textvisible")]
    public partial class ToolbarButton : UserControl
    {
        private bool _isSvg;

        public static readonly StyledProperty<ButtonId> IdProperty =
           AvaloniaProperty.Register<ToolbarButton, ButtonId>(nameof(Id), ButtonId.None);

        public static readonly StyledProperty<bool> ToggleProperty =
           AvaloniaProperty.Register<ToolbarButton, bool>(nameof(Toggle), false);

        public static readonly StyledProperty<string?> TextProperty =
            AvaloniaProperty.Register<ToolbarButton, string?>(nameof(Text), "Toolbutton");

        public static readonly StyledProperty<string?> PathProperty =
            AvaloniaProperty.Register<ToolbarButton, string?>(nameof(Path), null);

        public static readonly StyledProperty<bool> ToggledProperty =
            AvaloniaProperty.Register<ToolbarButton, bool>(nameof(Toggled), false);

        public static readonly StyledProperty<bool> ShowTextProperty =
            AvaloniaProperty.Register<ToolbarButton, bool>(nameof(ShowText), true);

        public static readonly StyledProperty<ObservableCollection<ButtonId>> ChildrenProperty =
            AvaloniaProperty.Register<ToolbarButton, ObservableCollection<ButtonId>>(nameof(Children));

        public static readonly StyledProperty<bool> AvailableProperty =
            AvaloniaProperty.Register<ToolbarButton, bool>(nameof(Available), false);

        public static readonly DirectProperty<ToolbarButton, bool> IsSvgProperty =
            AvaloniaProperty.RegisterDirect<ToolbarButton, bool>(nameof(IsSvg), o => o.IsSvg);



        public Toolstrip? Toolstrip { get; internal set; }

        public string? Path
        {
            get => GetValue(PathProperty);
            set => SetValue(PathProperty, value);
        }

        public bool Toggled
        {
            get => GetValue(ToggledProperty);
            set => SetValue(ToggledProperty, value);
        }

        public string? Text
        {
            set => SetValue(TextProperty, value);
            get => GetValue(TextProperty);
        }

        public ButtonId Id 
        { 
            get => GetValue(IdProperty); 
            set => SetValue(IdProperty, value); 
        }

        public bool Toggle
        {
            get => GetValue(ToggleProperty);
            set => SetValue(ToggleProperty, value);
        }

        public bool ShowText
        {
            get => GetValue(ShowTextProperty);
            set => SetValue(ShowTextProperty, value);
        }

        public ObservableCollection<ButtonId> Children 
        { 
            get => GetValue(ChildrenProperty);
            set => SetValue(ChildrenProperty, value);
        }

        public bool Available
        {
            get => GetValue(AvailableProperty);
            set => SetValue(AvailableProperty, value);
        }

        public string Description { get; set; } =
            "This is the description of a mouse button";

        public bool IsMouseDown = false;

        public bool IsSvg
        {
            get => _isSvg;
            private set => SetAndRaise(IsSvgProperty, ref _isSvg, value);
        }

        public event EventHandler<ButtonAction>? OnClick;

        public ToolbarButton()
        {
            InitializeComponent();

            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;

            InvalidateVisual();

            this.GetObservable(ToggledProperty).Subscribe(isToggled 
                => PseudoClasses.Set(":toggled", isToggled));

            this.GetObservable(AvailableProperty).Subscribe(isAvaiable 
                => PseudoClasses.Set(":available", isAvaiable));

            this.GetObservable(ShowTextProperty).Subscribe(showText => 
            {
                PART_TextContainer.IsVisible = showText;

                FluidPadding.SetReferencePadding(PART_ButtonContainer, showText ? new Thickness(5, 5, 5, 0) : new Thickness(10));

                PART_Layout.RowDefinitions[1].Height = showText
                    ? new GridLength(0.6, GridUnitType.Star)
                    : new GridLength(0);
            });
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == PathProperty)
            {
                IsSvg = string.Equals(
                    System.IO.Path.GetExtension(Path),
                    ".svg",
                    StringComparison.OrdinalIgnoreCase);

                if (!IsSvg) 
                    LoadImage();
            }
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            e.Pointer.Capture(this);

            IsMouseDown = true;

            PseudoClasses.Set(":clicked", true);
            
            e.Handled = true;
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            if (!IsMouseDown)
                return;

            IsMouseDown = false;
            

            PseudoClasses.Set(":clicked", false);

            if (Toggle)
            {
                InternalToggle();
            }
            else
            {
                ClickAction(ButtonAction.Click);
            }

            e.Handled = true;

            e.Pointer.Capture(null);
        }

        protected override void OnPointerEntered(PointerEventArgs e)
        {
            base.OnPointerEntered(e);

            PseudoClasses.Set(":hover", true);

            if (Toggle && Toggled)
                return;
        }

        protected override void OnPointerExited(PointerEventArgs e)
        {
            base.OnPointerExited(e);

            IsMouseDown = false;

            PseudoClasses.Set(":hover", false);
        }

        private void InternalToggle()
        {
            if (Toggled)
            {
                ReleaseToggle();
                return;
            }

            SetCurrentValue(ToggledProperty, true);
            ClickAction(ButtonAction.ToggleDown);
        }

        public void ReleaseToggle(bool soft = false)
        {
            if (!Toggled)
                return;

            SetCurrentValue(ToggledProperty, false);

            if (!soft)
                ClickAction(ButtonAction.ToggleUp);
        }

        public Bitmap ToImage(string icon)
        {
            var uri = new Uri($"avares://TitanControl/Assets/Images/{icon}");

            using var stream = AssetLoader.Open(uri);
            return new Bitmap(stream);
        }

        protected virtual void LoadImage()
        {
            if (Path is not null)
                ButtonImage.Source = ToImage(Path);
        }

        protected virtual void ClickAction(ButtonAction action)
        {
            OnClick?.Invoke(this, action);
        }

        private IBrush? FindResource(string key)
        {
            if (this.TryFindResource(
                key,
                ActualThemeVariant,
                out var found))
            {
                return (IBrush?)found;
            }

            return Brushes.Black;
        }
    }
}