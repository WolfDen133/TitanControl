using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using System;
using System.Numerics;
using TitanControl.Events.Control;
using Control = Avalonia.Controls.Control;

namespace TitanControl.Views.Controls.Layout.Grid
{
    public class GridLayout : Panel
    {
        public static readonly StyledProperty<int> RowsProperty =
           AvaloniaProperty.Register<GridLayout, int>(nameof(Rows), 12);

        public static readonly StyledProperty<int> ColumnsProperty =
            AvaloniaProperty.Register<GridLayout, int>(nameof(Columns), 12);


        public static readonly AttachedProperty<int> GridXProperty =
            AvaloniaProperty.RegisterAttached<GridLayout, Control, int>(
                "GridX",
                defaultValue: 0);

        public static readonly AttachedProperty<int> GridYProperty =
            AvaloniaProperty.RegisterAttached<GridLayout, Control, int>(
                "GridY",
                defaultValue: 0);

        public static readonly AttachedProperty<int> GridXSpanProperty =
            AvaloniaProperty.RegisterAttached<GridLayout, Control, int>(
                "GridXSpan",
                defaultValue: 1);

        public static readonly AttachedProperty<int> GridYSpanProperty =
            AvaloniaProperty.RegisterAttached<GridLayout, Control, int>(
                "GridYSpan",
                defaultValue: 1);


        public static int GetGridX(Control control) =>
            control.GetValue(GridXProperty);

        public static void SetGridX(Control control, int value) =>
            control.SetValue(GridXProperty, value);

        public static int GetGridXSpan(Control control) =>
            control.GetValue(GridXSpanProperty);

        public static void SetGridXSpan(Control control, int value) =>
            control.SetValue(GridXSpanProperty, value);

        public static int GetGridY(Control control) =>
            control.GetValue(GridYProperty);

        public static void SetGridY(Control control, int value) =>
            control.SetValue(GridYProperty, value);

        public static int GetGridYSpan(Control control) =>
            control.GetValue(GridYSpanProperty);

        public static void SetGridYSpan(Control control, int value) =>
            control.SetValue(GridYSpanProperty, value);

        public static readonly RoutedEvent<GridDoubleClickedEventArgs> GridDoubleClickedEvent =
            RoutedEvent.Register<GridLayout, GridDoubleClickedEventArgs>(
                nameof(GridDoubleClicked),
                RoutingStrategies.Bubble);


        public bool IsMouseDown = false;
        private Point MouseStart = new Point(0, 0);
        private Point MouseEnd = new Point(0, 0);


        private Vector2 cellDimensions { get; set; }

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

        public event EventHandler<GridDoubleClickedEventArgs> GridDoubleClicked
        {
            add => AddHandler(GridDoubleClickedEvent, value);
            remove => RemoveHandler(GridDoubleClickedEvent, value);
        }


        protected override void OnSizeChanged(SizeChangedEventArgs e)
        {
            base.OnSizeChanged(e);

            cellDimensions = new Vector2((float)Bounds.Width / Columns, (float)Bounds.Height / Rows);
        }

        protected override void OnDoubleTapped(TappedEventArgs e)
        {
            // Only react when the actual background of GridLayout was clicked.
            if (!ReferenceEquals(e.Source, this))
                return;

            RaiseEvent(
                new GridDoubleClickedEventArgs(
                    GridDoubleClickedEvent));

            e.Handled = true;
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            MouseStart = MouseEnd = e.GetPosition(this);
            IsMouseDown = true;

            base.OnPointerPressed(e);
        }

        protected override void OnPointerMoved(PointerEventArgs e)
        {
            if (IsMouseDown) MouseEnd = e.GetPosition(this);

            base.OnPointerMoved(e);
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            IsMouseDown = false;

            base.OnPointerReleased(e);
        }



        public Rect GetSelectedArea()
        {
            return CreateNormalizedRect
            (
                MouseStart,
                MouseEnd
            );
        }

        public Rect GetSelectedCoords()
        {
            Rect selectedArea = GetSelectedArea();

            double left = Math.Floor(
                selectedArea.Left / cellDimensions.X);

            double top = Math.Floor(
                selectedArea.Top / cellDimensions.Y);

            double right = Math.Ceiling(
                selectedArea.Right / cellDimensions.X);

            double bottom = Math.Ceiling(
                selectedArea.Bottom / cellDimensions.Y);

            // A simple click inside a cell should still select one cell.
            if (right <= left)
                right = left + 1;

            if (bottom <= top)
                bottom = top + 1;

            return new Rect(
                left,
                top,
                right - left,
                bottom - top);
        }

        public Rect GetSelectedCoordsArea()
        {
            Rect coords = GetSelectedCoords();

            return new Rect(
                coords.X * cellDimensions.X,
                coords.Y * cellDimensions.Y,
                coords.Width * cellDimensions.X,
                coords.Height * cellDimensions.Y);
        }

        public Point GetCoordsFromPoint(Point point, Point? offset = null)
        {
            if (offset is not Point of) of = new Point(0, 0);

            return new Point((int)(point.X / cellDimensions.X) + of.X, (int)(point.Y / cellDimensions.Y) + of.Y);
        }

        public Point GetPointFromCoords(Point coords, Point? offset = null)
        {
            if (offset is not Point of) of = new Point(0, 0);

            return new Point(cellDimensions.X * (coords.X + of.X), cellDimensions.Y * (coords.Y + of.Y));
        }

        protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
        {
            base.OnAttachedToLogicalTree(e);
        }

        private static Rect CreateNormalizedRect(
            Point start,
            Point end)
        {
            double left =
                Math.Min(start.X, end.X);

            double top =
                Math.Min(start.Y, end.Y);

            double right =
                Math.Max(start.X, end.X);

            double bottom =
                Math.Max(start.Y, end.Y);

            return new Rect(
                left,
                top,
                right - left,
                bottom - top);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            var columns = Math.Max(1, Columns);
            var rows = Math.Max(1, Rows);

            var cellWidth = double.IsFinite(availableSize.Width)
                ? availableSize.Width / columns
                : double.PositiveInfinity;

            var cellHeight = double.IsFinite(availableSize.Height)
                ? availableSize.Height / rows
                : double.PositiveInfinity;

            foreach (var child in Children)
            {
                var width = Math.Max(1, GetGridXSpan(child));
                var height = Math.Max(1, GetGridYSpan(child));

                child.Measure(new Size(cellWidth * width, cellHeight * height));
            }

            return new Size(
                double.IsFinite(availableSize.Width) ? availableSize.Width : 0,
                double.IsFinite(availableSize.Height) ? availableSize.Height : 0);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var columns = Math.Max(1, Columns);
            var rows = Math.Max(1, Rows);

            var cellWidth = finalSize.Width / columns;
            var cellHeight = finalSize.Height / rows;

            foreach (var child in Children)
            {
                var x = GetGridX(child);
                var y = GetGridY(child);
                var width = GetGridXSpan(child);
                var height = GetGridYSpan(child);

                child.Arrange(new Rect(
                    x * cellWidth,
                    y * cellHeight,
                    width * cellWidth,
                    height * cellHeight));
            }

            return finalSize;
        }
    }
}
