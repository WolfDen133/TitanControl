using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.Collections.ObjectModel;
using TitanControl.Views.Controls.Toolbar.Button;

namespace TitanControl.Views.Controls.Toolbar.Buttons
{
    [PseudoClasses(":hover", ":clicked", ":toggled")]
    public partial class ToolbarButton : UserControl
    {
        private bool _isSvg;

        public static readonly StyledProperty<string?> PathProperty =
            AvaloniaProperty.Register<ToolbarButton, string?>(nameof(Path), null);

        public static readonly DirectProperty<ToolbarButton, bool> IsSvgProperty =
            AvaloniaProperty.RegisterDirect<ToolbarButton, bool>(nameof(IsSvg), o => o.IsSvg);



        /// <summary>
        /// Set automatically by the containing Toolstrip when the menu tree is loaded.
        /// </summary>
        public Toolstrip? Toolstrip { get; internal set; }

        public string? Path
        {
            get => GetValue(PathProperty);
            set => SetValue(PathProperty, value);
        }

        public int ID { get; set; }

        /// <summary>
        /// Child menu buttons belonging to this button.
        ///
        /// These are real ToolbarButton instances rather than IDs. The Toolstrip
        /// discovers this hierarchy on load and attaches the nested buttons to its
        /// visual collection so it can control their visibility.
        /// </summary>
        public ObservableCollection<ToolbarButton> Children { get; } = new();

        public string Text
        {
            set => TextLabel.Content = value;
            get => (string?)TextLabel.Content ?? "Button";
        }

        public string Description { get; set; } =
            "This is the description of a mouse button";

        public bool IsMouseDown = false;

        public bool Toggle
        {
            get;
            set;
        } = false;

        private bool toggled = false;

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

        public void SetSize(int size)
        {
            Width = Height = size;
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

            if (Toggle && toggled)
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
            if (toggled)
            {
                ReleaseToggle();
                return;
            }

            toggled = true;
            PseudoClasses.Set(":toggled", true);

            ClickAction(ButtonAction.ToggleDown);
        }

        public void ReleaseToggle(bool soft = false)
        {
            if (!toggled)
                return;

            toggled = false;
            PseudoClasses.Set(":toggled", false);

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