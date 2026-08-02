using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.LogicalTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using TitanControl.Logging;

namespace TitanControl.Controls.Layout
{
    public class GridLayout : Panel
    {
        public static readonly StyledProperty<int> RowsProperty =
           AvaloniaProperty.Register<GridLayout, int>(nameof(Rows), 12);

        public static readonly StyledProperty<int> ColumnsProperty =
            AvaloniaProperty.Register<GridLayout, int>(nameof(Columns), 12);

        private bool isMouseDown = false;
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

        protected override void OnSizeChanged(SizeChangedEventArgs e)
        {
            base.OnSizeChanged(e);

            cellDimensions = new Vector2((float)Bounds.Width / Columns, (float)Bounds.Height / Rows);
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            MouseStart = MouseEnd = e.GetPosition(this);
            isMouseDown = true;

            base.OnPointerPressed(e);
        }

        protected override void OnPointerMoved(PointerEventArgs e)
        {
            if (isMouseDown) MouseEnd = e.GetPosition(this);

            base.OnPointerMoved(e);
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            isMouseDown = false;

            base.OnPointerReleased(e);

            MouseStart = MouseEnd = new Point();
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
            // Measure children unconstrained to determine natural cell size
            double maxChildWidth = 0;
            double maxChildHeight = 0;

            foreach (var child in Children)
            {
                child.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                var d = child.DesiredSize;
                maxChildWidth = Math.Max(maxChildWidth, d.Width);
                maxChildHeight = Math.Max(maxChildHeight, d.Height);
            }

            int cols = Columns;
            int rows = Rows;

            if (cols <= 0 && rows <= 0)
                cols = (int)Math.Ceiling(Math.Sqrt(Math.Max(1, Children.Count)));

            if (cols <= 0)
                cols = Math.Max(1, (int)Math.Ceiling((double)Children.Count / rows));

            if (rows <= 0)
                rows = Math.Max(1, (int)Math.Ceiling((double)Children.Count / cols));

            var desired = new Size(cols * maxChildWidth, rows * maxChildHeight);

            // Never return invalid size
            if (double.IsInfinity(availableSize.Width) && double.IsInfinity(availableSize.Height))
                return desired;

            return new Size(
                double.IsInfinity(availableSize.Width) ? desired.Width : availableSize.Width,
                double.IsInfinity(availableSize.Height) ? desired.Height : availableSize.Height);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            int cols = Columns;
            int rows = Rows;

            if (cols <= 0 && rows <= 0)
                cols = (int)Math.Ceiling(Math.Sqrt(Children.Count));

            if (cols <= 0)
                cols = Math.Max(1, (int)Math.Ceiling((double)Children.Count / rows));

            if (rows <= 0)
                rows = Math.Max(1, (int)Math.Ceiling((double)Children.Count / cols));

            double cellWidth = finalSize.Width / cols;
            double cellHeight = finalSize.Height / rows;

            for (int i = 0; i < Children.Count; i++)
            {
                var child = Children[i];
                int row = i / cols;
                int col = i % cols;

                var targetBounds = new Rect(
                    col * cellWidth,
                    row * cellHeight,
                    cellWidth,
                    cellHeight);

                child.Arrange(targetBounds);
            }

            return finalSize;
        }
    }
}
