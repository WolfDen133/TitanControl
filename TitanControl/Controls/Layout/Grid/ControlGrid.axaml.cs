using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using System;
using System.Diagnostics;
using System.Numerics;
using TitanControl.Controls.Handle;
using TitanControl.Logging;

namespace TitanControl.Controls.Layout
{
    public partial class ControlGrid : UserControl
    {
        public static readonly StyledProperty<int> RowsProperty =
            AvaloniaProperty.Register<GridLayout, int>(nameof(Rows), 12);

        public static readonly StyledProperty<int> ColumnsProperty =
            AvaloniaProperty.Register<GridLayout, int>(nameof(Columns), 12);

        public bool SnapSelection = false;

        public int Rows
        {
            get => GetValue(RowsProperty);
            set => SetValue(RowsProperty, value);
        }

        public int Columns
        {
            get => GetValue(ColumnsProperty);
            set => SetValue(ColumnsProperty, value);
        }
        public ControlGrid()
        {
            InitializeComponent();
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            GridLayout.PointerPressed += (_, _) =>
            {
                UpdateSelectionRectangle(!SnapSelection ? GridLayout.GetSelectedArea() : GridLayout.GetSelectedCoordsArea());

                Selection.IsVisible = true;
            };

            GridLayout.PointerMoved += (_, _) =>
            {
                UpdateSelectionRectangle(!SnapSelection ? GridLayout.GetSelectedArea() : GridLayout.GetSelectedCoordsArea());
            };

            GridLayout.PointerReleased += (_, _) =>
            {
                Selection.IsVisible = false;
            };
        }

        private void UpdateSelectionRectangle(Rect bounds)
        {
            Canvas.SetLeft(
                Selection,
                bounds.X);

            Canvas.SetTop(
                Selection,
                bounds.Y);

            Selection.Width =
                bounds.Width;

            Selection.Height =
                bounds.Height;
        }

        public void AddControl(BaseHandleControl control)
        {
            GridLayout.Children.Add(control);
        }
    }
}