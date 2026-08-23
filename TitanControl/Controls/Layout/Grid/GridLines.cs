using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using System;

namespace TitanControl.Controls.Layout
{
    internal class GridLines : Control
    {
        public static readonly StyledProperty<int> ColumnsProperty =
            AvaloniaProperty.Register<GridLines, int>(nameof(Columns));

        public static readonly StyledProperty<int> RowsProperty =
            AvaloniaProperty.Register<GridLines, int>(nameof(Rows));

        public static readonly StyledProperty<double> ThicknessProperty =
            AvaloniaProperty.Register<GridLines, double>(nameof(Thickness), 1);

        public static readonly StyledProperty<IBrush> BrushProperty =
            AvaloniaProperty.Register<GridLines, IBrush>(nameof(Brush), Brushes.White);

        public int Columns
        {
            get => GetValue(ColumnsProperty);
            set => SetValue(ColumnsProperty, value);
        }

        public int Rows
        {
            get => GetValue(RowsProperty);
            set => SetValue(RowsProperty, value);
        }

        public double Thickness
        {
            get => GetValue(ThicknessProperty);
            set => SetValue(ThicknessProperty, value);
        }

        public IBrush Brush
        {
            get => GetValue(BrushProperty);
            set => SetValue(BrushProperty, value);
        }

        public override void Render(DrawingContext context)
        {
            double w = Bounds.Width;
            double h = Bounds.Height;

            double cellW = w / Columns;
            double cellH = h / Rows;

            Pen pen = new Pen(Brush, Thickness);

            // 🔴 ONLY internal vertical lines
            for (int c = 1; c < Columns; c++)
            {
                double x = Math.Round(c * cellW);
                context.DrawLine(pen, new Point(x, 0), new Point(x, h));
            }

            // 🔵 ONLY internal horizontal lines
            for (int r = 1; r < Rows; r++)
            {
                double y = Math.Round(r * cellH);
                context.DrawLine(pen, new Point(0, y), new Point(w, y));
            }
        }
    }
}

