using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using System;
using TitanControl.Events.Control;
using TitanControl.Views.State;
using Control = Avalonia.Controls.Control;

namespace TitanControl.Views.Controls.Layout.Grid;


// AI Refined
public class GridLayout : Panel
{
    public static readonly StyledProperty<int> RowsProperty =
        AvaloniaProperty.Register<GridLayout, int>(
            nameof(Rows),
            defaultValue: 12);

    public static readonly StyledProperty<int> ColumnsProperty =
        AvaloniaProperty.Register<GridLayout, int>(
            nameof(Columns),
            defaultValue: 12);

    public static readonly StyledProperty<bool> IsSelectionEnabledProperty =
        AvaloniaProperty.Register<GridLayout, bool>(
            nameof(IsSelectionEnabled),
            defaultValue: false);

    public static readonly StyledProperty<bool> SelectOverProperty =
        AvaloniaProperty.Register<GridLayout, bool>(
            nameof(SelectOver),
            defaultValue: false);

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

    public static readonly RoutedEvent<GridDoubleClickedEventArgs>
        GridDoubleClickedEvent =
            RoutedEvent.Register<GridLayout, GridDoubleClickedEventArgs>(
                nameof(GridDoubleClicked),
                RoutingStrategies.Bubble);

    private Point _selectionStart;
    private Point _selectionEnd;
    private bool _isSelecting;

    static GridLayout()
    {
        // These properties exist on GridLayout itself.
        AffectsMeasure<GridLayout>(
            RowsProperty,
            ColumnsProperty);

        // GridX/GridY are set on a ContentPresenter and affect its parent panel.
        AffectsParentArrange<GridLayout>(
            GridXProperty,
            GridYProperty);

        // Span changes affect how the child is measured. A measure invalidation
        // automatically results in another arrange pass.
        AffectsParentMeasure<GridLayout>(
            GridXSpanProperty,
            GridYSpanProperty);
    }

    public GridLayout()
    {
        // handledEventsToo allows selection to work when a generated child
        // handles its own pointer event.
        AddHandler(
            PointerPressedEvent,
            OnSelectionPointerPressed,
            RoutingStrategies.Bubble,
            handledEventsToo: true);

        AddHandler(
            PointerMovedEvent,
            OnSelectionPointerMoved,
            RoutingStrategies.Bubble,
            handledEventsToo: true);

        AddHandler(
            PointerReleasedEvent,
            OnSelectionPointerReleased,
            RoutingStrategies.Bubble,
            handledEventsToo: true);
    }

    protected override void OnPropertyChanged(
    AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IsSelectionEnabledProperty &&
            !IsSelectionEnabled)
        {
            _selectionStart = default;
            _selectionEnd = default;
            _isSelecting = false;
        }
    }

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

    public bool IsSelectionEnabled
    {
        get => GetValue(IsSelectionEnabledProperty);
        set => SetValue(IsSelectionEnabledProperty, value);
    }

    public bool SelectOver
    {
        get => GetValue(SelectOverProperty);
        set => SetValue(SelectOverProperty, value);
    }

    public bool IsSelecting => _isSelecting;

    public event EventHandler? SelectionStarted;
    public event EventHandler? SelectionChanged;
    public event EventHandler? SelectionCompleted;

    public event EventHandler<GridDoubleClickedEventArgs> GridDoubleClicked
    {
        add => AddHandler(GridDoubleClickedEvent, value);
        remove => RemoveHandler(GridDoubleClickedEvent, value);
    }

    public static int GetGridX(Control control) =>
        control.GetValue(GridXProperty);

    public static void SetGridX(Control control, int value) =>
        control.SetValue(GridXProperty, value);

    public static int GetGridY(Control control) =>
        control.GetValue(GridYProperty);

    public static void SetGridY(Control control, int value) =>
        control.SetValue(GridYProperty, value);

    public static int GetGridXSpan(Control control) =>
        control.GetValue(GridXSpanProperty);

    public static void SetGridXSpan(Control control, int value) =>
        control.SetValue(GridXSpanProperty, value);

    public static int GetGridYSpan(Control control) =>
        control.GetValue(GridYSpanProperty);

    public static void SetGridYSpan(Control control, int value) =>
        control.SetValue(GridYSpanProperty, value);

    protected override void OnDoubleTapped(TappedEventArgs e)
    {
        // Allow background double-clicks only.
        if (!ReferenceEquals(e.Source, this))
            return;

        RaiseEvent(
            new GridDoubleClickedEventArgs(
                GridDoubleClickedEvent));

        e.Handled = true;
    }

    private void OnSelectionPointerPressed(
        object? sender,
        PointerPressedEventArgs e)
    {
        if (!IsSelectionEnabled)
            return;

        var currentPoint = e.GetCurrentPoint(this);

        if (currentPoint.Properties.PointerUpdateKind !=
            PointerUpdateKind.LeftButtonPressed)
        {
            return;
        }

        // When false, a child cannot initiate box selection.
        if (!SelectOver && !ReferenceEquals(e.Source, this))
            return;

        _selectionStart = ClampToBounds(e.GetPosition(this));
        _selectionEnd = _selectionStart;
        _isSelecting = true;

        e.Pointer.Capture(this);

        SelectionStarted?.Invoke(this, EventArgs.Empty);
        SelectionChanged?.Invoke(this, EventArgs.Empty);

        e.Handled = true;
    }

    private void OnSelectionPointerMoved(
        object? sender,
        PointerEventArgs e)
    {
        if (!_isSelecting || !IsSelectionEnabled)
            return;

        _selectionEnd = ClampToBounds(e.GetPosition(this));

        SelectionChanged?.Invoke(this, EventArgs.Empty);

        e.Handled = true;
    }

    private void OnSelectionPointerReleased(
        object? sender,
        PointerReleasedEventArgs e)
    {
        if (!_isSelecting || !IsSelectionEnabled)
            return;

        _selectionEnd = ClampToBounds(e.GetPosition(this));
        _isSelecting = false;

        e.Pointer.Capture(null);

        SelectionChanged?.Invoke(this, EventArgs.Empty);
        SelectionCompleted?.Invoke(this, EventArgs.Empty);

        e.Handled = true;
    }

    public Rect GetSelectedArea()
    {
        var selectedArea = CreateNormalizedRect(
            _selectionStart,
            _selectionEnd);

        return SelectOver
            ? selectedArea
            : ConstrainToUnoccupiedArea(selectedArea, useGridCoordinates: false);
    }

    public Rect GetSelectedCoords()
    {
        // Calculate from the raw pointer rectangle. Pixel-space clipping is not
        // used here because snapping can expand the rectangle back into a child.
        var selectedArea = CreateNormalizedRect(
            _selectionStart,
            _selectionEnd);

        var cellSize = GetCellSize();

        var left = Math.Floor(selectedArea.Left / cellSize.Width);
        var top = Math.Floor(selectedArea.Top / cellSize.Height);
        var right = Math.Ceiling(selectedArea.Right / cellSize.Width);
        var bottom = Math.Ceiling(selectedArea.Bottom / cellSize.Height);

        if (right <= left)
            right = left + 1;

        if (bottom <= top)
            bottom = top + 1;

        left = Math.Clamp(left, 0, Math.Max(0, Columns - 1));
        top = Math.Clamp(top, 0, Math.Max(0, Rows - 1));
        right = Math.Clamp(right, left + 1, Math.Max(1, Columns));
        bottom = Math.Clamp(bottom, top + 1, Math.Max(1, Rows));

        var selectedCoordinates = new Rect(
            left,
            top,
            right - left,
            bottom - top);

        return SelectOver
            ? selectedCoordinates
            : ConstrainToUnoccupiedArea(
                selectedCoordinates,
                useGridCoordinates: true);
    }

    public Rect GetSelectedCoordsArea()
    {
        var coordinates = GetSelectedCoords();
        var cellSize = GetCellSize();

        return new Rect(
            coordinates.X * cellSize.Width,
            coordinates.Y * cellSize.Height,
            coordinates.Width * cellSize.Width,
            coordinates.Height * cellSize.Height);
    }

    public Point GetCoordsFromPoint(
        Point point,
        Point? offset = null)
    {
        var actualOffset = offset ?? default;
        var cellSize = GetCellSize();

        return new Point(
            Math.Floor(point.X / cellSize.Width) + actualOffset.X,
            Math.Floor(point.Y / cellSize.Height) + actualOffset.Y);
    }

    public Point GetPointFromCoords(
        Point coordinates,
        Point? offset = null)
    {
        var actualOffset = offset ?? default;
        var cellSize = GetCellSize();

        return new Point(
            cellSize.Width * (coordinates.X + actualOffset.X),
            cellSize.Height * (coordinates.Y + actualOffset.Y));
    }

    private Size GetCellSize()
    {
        var columns = Math.Max(1, Columns);
        var rows = Math.Max(1, Rows);

        return new Size(
            Math.Max(1, Bounds.Width / columns),
            Math.Max(1, Bounds.Height / rows));
    }

    private Point ClampToBounds(Point point)
    {
        return new Point(
            Math.Clamp(point.X, 0, Bounds.Width),
            Math.Clamp(point.Y, 0, Bounds.Height));
    }

    /// <summary>
    /// Shrinks a selection rectangle so it cannot overlap an existing child.
    /// The edge anchored at the initial pointer position remains fixed.
    /// </summary>
    private Rect ConstrainToUnoccupiedArea(
        Rect candidate,
        bool useGridCoordinates)
    {
        if (candidate.Width <= 0 || candidate.Height <= 0)
            return candidate;

        var draggingRight = _selectionEnd.X >= _selectionStart.X;
        var draggingDown = _selectionEnd.Y >= _selectionStart.Y;

        foreach (var child in Children)
        {
            if (!child.IsVisible)
                continue;

            var occupiedArea = useGridCoordinates
                ? new Rect(
                    GetGridX(child),
                    GetGridY(child),
                    Math.Max(1, GetGridXSpan(child)),
                    Math.Max(1, GetGridYSpan(child)))
                : child.Bounds;

            if (!IntersectsWith(candidate, occupiedArea))
                continue;

            candidate = ClipAroundOccupiedArea(
                candidate,
                occupiedArea,
                draggingRight,
                draggingDown);

            if (candidate.Width <= 0 || candidate.Height <= 0)
                return new Rect(
                    candidate.X,
                    candidate.Y,
                    Math.Max(0, candidate.Width),
                    Math.Max(0, candidate.Height));
        }

        return candidate;
    }

    private static Rect ClipAroundOccupiedArea(
        Rect selection,
        Rect occupied,
        bool draggingRight,
        bool draggingDown)
    {
        // There are two valid ways to avoid a colliding control: restrict the
        // horizontal edge or restrict the vertical edge. Keep whichever option
        // preserves the larger selectable area.
        var horizontal = draggingRight
            ? new Rect(
                selection.Left,
                selection.Top,
                Math.Max(0, occupied.Left - selection.Left),
                selection.Height)
            : new Rect(
                occupied.Right,
                selection.Top,
                Math.Max(0, selection.Right - occupied.Right),
                selection.Height);

        var vertical = draggingDown
            ? new Rect(
                selection.Left,
                selection.Top,
                selection.Width,
                Math.Max(0, occupied.Top - selection.Top))
            : new Rect(
                selection.Left,
                occupied.Bottom,
                selection.Width,
                Math.Max(0, selection.Bottom - occupied.Bottom));

        return GetArea(horizontal) >= GetArea(vertical)
            ? horizontal
            : vertical;
    }

    // Avalonia.Rect uses Intersects; this explicit IntersectsWith equivalent
    // makes edge contact legal while rejecting any positive-area overlap.
    private static bool IntersectsWith(Rect first, Rect second)
    {
        return first.Width > 0 &&
               first.Height > 0 &&
               second.Width > 0 &&
               second.Height > 0 &&
               first.Left < second.Right &&
               first.Right > second.Left &&
               first.Top < second.Bottom &&
               first.Bottom > second.Top;
    }

    private static double GetArea(Rect rectangle) =>
        rectangle.Width * rectangle.Height;

    private static Rect CreateNormalizedRect(
        Point start,
        Point end)
    {
        var left = Math.Min(start.X, end.X);
        var top = Math.Min(start.Y, end.Y);
        var right = Math.Max(start.X, end.X);
        var bottom = Math.Max(start.Y, end.Y);

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

            child.Measure(
                new Size(
                    cellWidth * width,
                    cellHeight * height));
        }

        return new Size(
            double.IsFinite(availableSize.Width)
                ? availableSize.Width
                : 0,
            double.IsFinite(availableSize.Height)
                ? availableSize.Height
                : 0);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var columns = Math.Max(1, Columns);
        var rows = Math.Max(1, Rows);

        var cellWidth = finalSize.Width / columns;
        var cellHeight = finalSize.Height / rows;

        foreach (var child in Children)
        {
            var x = Math.Max(0, GetGridX(child));
            var y = Math.Max(0, GetGridY(child));
            var width = Math.Max(1, GetGridXSpan(child));
            var height = Math.Max(1, GetGridYSpan(child));

            child.Arrange(
                new Rect(
                    x * cellWidth,
                    y * cellHeight,
                    width * cellWidth,
                    height * cellHeight));
        }

        return finalSize;
    }
}
