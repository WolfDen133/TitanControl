using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using System;
using System.Diagnostics;
using TitanControl.Events.Control;
using TitanControl.Logging;
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
    private Point _pendingPointerStart;
    private Rect? _startingOccupiedArea;
    private bool _isPointerDown;
    private bool _hasSelection;
    private enum ExitSide { None, Left, Right, Top, Bottom }
    private ExitSide _exitSide;

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
            CancelSelection();
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

    public bool IsSelecting => _isPointerDown;

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

        var pointerPosition = ClampToBounds(e.GetPosition(this));

        _isPointerDown = true;
        _hasSelection = false;
        _exitSide = ExitSide.None;
        _pendingPointerStart = pointerPosition;
        _startingOccupiedArea = SelectOver
            ? null
            : FindOccupiedArea(pointerPosition);

        // Do not instantiate the real selection coordinates while the pointer
        // is inside a child. They are created only after a child edge is exited.
        if (_startingOccupiedArea is null)
        {
            BeginSelection(pointerPosition, pointerPosition);

            SelectionStarted?.Invoke(this, EventArgs.Empty);
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        e.Pointer.Capture(this);

        e.Handled = true;
    }

    private void OnSelectionPointerMoved(
        object? sender,
        PointerEventArgs e)
    {
        if (!_isPointerDown || !IsSelectionEnabled)
            return;

        var pointerPosition = ClampToBounds(e.GetPosition(this));

        if (!_hasSelection)
        {
            if (_startingOccupiedArea is not Rect occupiedArea ||
                !TryCreateExitAnchor(
                    _pendingPointerStart,
                    pointerPosition,
                    occupiedArea,
                    out var selectionAnchor))
            {
                e.Handled = true;
                return;
            }

            BeginSelection(selectionAnchor, pointerPosition);
            SelectionStarted?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            _selectionEnd = pointerPosition;
        }

        SelectionChanged?.Invoke(this, EventArgs.Empty);

        e.Handled = true;
    }

    private void OnSelectionPointerReleased(
        object? sender,
        PointerReleasedEventArgs e)
    {
        if (!_isPointerDown || !IsSelectionEnabled)
            return;

        var pointerPosition = ClampToBounds(e.GetPosition(this));
        var startedOnRelease = false;

        if (!_hasSelection &&
            _startingOccupiedArea is Rect occupiedArea &&
            TryCreateExitAnchor(
                _pendingPointerStart,
                pointerPosition,
                occupiedArea,
                out var selectionAnchor))
        {
            BeginSelection(selectionAnchor, pointerPosition);
            startedOnRelease = true;
        }
        else if (_hasSelection)
        {
            _selectionEnd = pointerPosition;
        }

        _isPointerDown = false;

        e.Pointer.Capture(null);

        if (_hasSelection)
        {
            if (startedOnRelease)
                SelectionStarted?.Invoke(this, EventArgs.Empty);

            SelectionChanged?.Invoke(this, EventArgs.Empty);
            SelectionCompleted?.Invoke(this, EventArgs.Empty);
        }

        e.Handled = true;
    }

    public Rect GetSelectedArea()
    {
        if (!_hasSelection)
            return default;

        var selectedArea = CreateNormalizedRect(
            _selectionStart,
            _selectionEnd);

        return SelectOver
            ? selectedArea
            : ConstrainToUnoccupiedArea(selectedArea, useGridCoordinates: false);
    }

    public Rect GetSelectedCoords()
    {
        if (!_hasSelection)
            return default;

        // Calculate from the raw pointer rectangle. Pixel-space clipping is not
        // used here because snapping can expand the rectangle back into a child.
        var selectedArea = CreateNormalizedRect(
            _selectionStart,
            _selectionEnd);

        var cellSize = GetCellSize();

        var left = Math.Floor(SnapGridEdge(selectedArea.Left / cellSize.Width));
        var top = Math.Floor(SnapGridEdge(selectedArea.Top / cellSize.Height));
        var right = Math.Ceiling(SnapGridEdge(selectedArea.Right / cellSize.Width));
        var bottom = Math.Ceiling(SnapGridEdge(selectedArea.Bottom / cellSize.Height));

        if (right <= left)
            right = left + 1;

        if (bottom <= top)
            bottom = top + 1;

        left = Math.Clamp(left, 0, Math.Max(0, Columns - 1));
        top = Math.Clamp(top, 0, Math.Max(0, Rows - 1));
        right = Math.Clamp(right, left + 1, Math.Max(1, Columns));
        bottom = Math.Clamp(bottom, top + 1, Math.Max(1, Rows));

        // The selected area must remain on the side where the drag first exited.
        // In particular, never force a minimum cell back into the starting child.
        if (!SelectOver && _exitSide != ExitSide.None)
        {
            var anchor = GetSelectionAnchor(useGridCoordinates: true);
            switch (_exitSide)
            {
                case ExitSide.Left: right = Math.Min(right, anchor.X); break;
                case ExitSide.Right: left = Math.Max(left, anchor.X); break;
                case ExitSide.Top: bottom = Math.Min(bottom, anchor.Y); break;
                case ExitSide.Bottom: top = Math.Max(top, anchor.Y); break;
            }

            if (right <= left || bottom <= top)
                return default;
        }

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

        if (coordinates.Width <= 0 || coordinates.Height <= 0)
            return default;

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
            Bounds.Width > 0 ? Bounds.Width / columns : 1,
            Bounds.Height > 0 ? Bounds.Height / rows : 1);
    }

    public Rect GetCellBounds(int x, int y, int width = 1, int height = 1)
    {
        var columns = Math.Max(1, Columns);
        var rows = Math.Max(1, Rows);
        var cellSize = GetCellSize();

        return new Rect(
            x * cellSize.Width,
            y * cellSize.Height,
            Math.Max(1, width) * cellSize.Width,
            Math.Max(1, height) * cellSize.Height);
    }

    private Point ClampToBounds(Point point)
    {
        return new Point(
            Math.Clamp(point.X, 0, Bounds.Width),
            Math.Clamp(point.Y, 0, Bounds.Height));
    }

    private void BeginSelection(Point start, Point end)
    {
        _selectionStart = start;
        _selectionEnd = end;
        _hasSelection = true;
    }

    private void CancelSelection()
    {
        _selectionStart = default;
        _selectionEnd = default;
        _pendingPointerStart = default;
        _startingOccupiedArea = null;
        _isPointerDown = false;
        _hasSelection = false;
        _exitSide = ExitSide.None;
    }

    private Rect? FindOccupiedArea(Point point)
    {
        // Search backwards so the top-most generated item wins if controls
        // happen to overlap.
        for (var index = Children.Count - 1; index >= 0; index--)
        {
            var child = Children[index];

            var area = GetOccupiedArea(child, useGridCoordinates: false);
            if (child.IsVisible && area.Contains(point))
                return area;
        }

        return null;
    }

    private bool TryCreateExitAnchor(
        Point pointerStart,
        Point pointerEnd,
        Rect occupiedArea,
        out Point anchor)
    {
        anchor = default;

        // Still inside the control: there is no selectable area yet.
        if (IsInside(occupiedArea, pointerEnd))
            return false;

        var deltaX = pointerEnd.X - pointerStart.X;
        var deltaY = pointerEnd.Y - pointerStart.Y;
        var horizontalExitTime = double.PositiveInfinity;
        var verticalExitTime = double.PositiveInfinity;
        var horizontalEdge = pointerStart.X;
        var verticalEdge = pointerStart.Y;

        if (deltaX > 0 && pointerEnd.X > occupiedArea.Right)
        {
            horizontalEdge = occupiedArea.Right;
            horizontalExitTime =
                (occupiedArea.Right - pointerStart.X) / deltaX;
        }
        else if (deltaX < 0 && pointerEnd.X < occupiedArea.Left)
        {
            horizontalEdge = occupiedArea.Left;
            horizontalExitTime =
                (occupiedArea.Left - pointerStart.X) / deltaX;
        }

        if (deltaY > 0 && pointerEnd.Y > occupiedArea.Bottom)
        {
            verticalEdge = occupiedArea.Bottom;
            verticalExitTime =
                (occupiedArea.Bottom - pointerStart.Y) / deltaY;
        }
        else if (deltaY < 0 && pointerEnd.Y < occupiedArea.Top)
        {
            verticalEdge = occupiedArea.Top;
            verticalExitTime =
                (occupiedArea.Top - pointerStart.Y) / deltaY;
        }

        if (double.IsPositiveInfinity(horizontalExitTime) &&
            double.IsPositiveInfinity(verticalExitTime))
        {
            return false;
        }

        // For a diagonal drag, use the first side crossed by the pointer ray.
        // The unaffected coordinate remains the original mouse-down position.
        anchor = horizontalExitTime <= verticalExitTime
            ? new Point(horizontalEdge, pointerStart.Y)
            : new Point(pointerStart.X, verticalEdge);

        _exitSide = horizontalExitTime <= verticalExitTime
            ? (deltaX < 0 ? ExitSide.Left : ExitSide.Right)
            : (deltaY < 0 ? ExitSide.Top : ExitSide.Bottom);

        return true;
    }

    private static bool IsInside(Rect area, Point point)
    {
        return point.X > area.Left &&
               point.X < area.Right &&
               point.Y > area.Top &&
               point.Y < area.Bottom;
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
            return default;

        if (!SelectOver && _exitSide != ExitSide.None)
        {
            var anchor = GetSelectionAnchor(useGridCoordinates);
            var left = candidate.Left;
            var top = candidate.Top;
            var right = candidate.Right;
            var bottom = candidate.Bottom;
            switch (_exitSide)
            {
                case ExitSide.Left: right = Math.Min(right, anchor.X); break;
                case ExitSide.Right: left = Math.Max(left, anchor.X); break;
                case ExitSide.Top: bottom = Math.Min(bottom, anchor.Y); break;
                case ExitSide.Bottom: top = Math.Max(top, anchor.Y); break;
            }
            if (right <= left || bottom <= top)
                return default;
            candidate = new Rect(left, top, right - left, bottom - top);
        }

        var draggingRight = _selectionEnd.X >= _selectionStart.X;
        var draggingDown = _selectionEnd.Y >= _selectionStart.Y;

        foreach (var child in Children)
        {
            if (!child.IsVisible)
                continue;

            var occupiedArea = GetOccupiedArea(child, useGridCoordinates);

            if (!IntersectsWith(candidate, occupiedArea))
                continue;

            // A candidate completely contained by a child has no valid
            // unoccupied portion. Returning an empty rectangle also prevents
            // the old one-axis-locked result.
            if (Contains(occupiedArea, candidate))
                return default;

            var candidateAnchor = GetSelectionAnchor(useGridCoordinates);

            // This is a defensive fallback for an occupied starting point. In
            // normal pointer operation the pending-anchor logic above prevents
            // this path from being reached.
            if (IsInside(occupiedArea, candidateAnchor))
                return default;

            candidate = ClipAroundOccupiedArea(
                candidate,
                occupiedArea,
                draggingRight,
                draggingDown);

            if (candidate.Width <= 0 || candidate.Height <= 0)
                return default;
        }

        return candidate;
    }

    private Point GetSelectionAnchor(bool useGridCoordinates)
    {
        if (!useGridCoordinates)
            return _selectionStart;

        var cellSize = GetCellSize();

        return new Point(
            SnapGridEdge(_selectionStart.X / cellSize.Width),
            SnapGridEdge(_selectionStart.Y / cellSize.Height));
    }

    private static double SnapGridEdge(double value)
    {
        var nearest = Math.Round(value);
        return Math.Abs(value - nearest) < 1e-9 ? nearest : value;
    }

    private Rect GetOccupiedArea(Control child, bool useGridCoordinates)
    {
        var area = new Rect(
            Math.Max(0, GetGridX(child)),
            Math.Max(0, GetGridY(child)),
            Math.Max(1, GetGridXSpan(child)),
            Math.Max(1, GetGridYSpan(child)));
        if (useGridCoordinates)
            return area;
        var cell = GetCellSize();
        return new Rect(area.X * cell.Width, area.Y * cell.Height,
            area.Width * cell.Width, area.Height * cell.Height);
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

    private static bool Contains(Rect outer, Rect inner)
    {
        return inner.Left >= outer.Left &&
               inner.Top >= outer.Top &&
               inner.Right <= outer.Right &&
               inner.Bottom <= outer.Bottom;
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