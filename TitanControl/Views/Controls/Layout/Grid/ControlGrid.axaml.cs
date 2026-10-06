using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.VisualTree;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using TitanControl.Converters;
using TitanControl.Events.Workspace;
using TitanControl.Helper;
using TitanControl.Logging;
using TitanControl.ViewModels.Workspace;
using TitanControl.ViewModels.Workspace.Controls;
using TitanControl.ViewModels.Workspace.Controls.Handle;
using TitanControl.Views.State;

namespace TitanControl.Views.Controls.Layout.Grid;

[PseudoClasses(":selecting")]
public partial class ControlGrid : UserControl
{
    private GridLayout? _gridLayout;

    private static readonly Transitions FadeOutTransitions =
    [
        new DoubleTransition
        {
            Property = Visual.OpacityProperty,
            Duration = TimeSpan.FromMilliseconds(120)
        }
    ];

    public static readonly StyledProperty<string?> TopLeftTextProperty =
    AvaloniaProperty.Register<ControlGrid, string?>(
        nameof(TopLeftText), null);

    public static readonly StyledProperty<int> RowsProperty =
        AvaloniaProperty.Register<ControlGrid, int>(
            nameof(Rows),
            defaultValue: 12);

    public static readonly StyledProperty<int> ColumnsProperty =
        AvaloniaProperty.Register<ControlGrid, int>(
            nameof(Columns),
            defaultValue: 12);

    public static readonly StyledProperty<bool> DisplayLinesProperty =
        AvaloniaProperty.Register<ControlGrid, bool>(
            nameof(DisplayLines),
            defaultValue: true);

    public static readonly StyledProperty<IEnumerable<IWorkspaceControl>> ControlsProperty =
        AvaloniaProperty.Register<ControlGrid, IEnumerable<IWorkspaceControl>>(
            nameof(Controls),
            defaultValue: Array.Empty<IWorkspaceControl>());

    public static readonly StyledProperty<bool> SelectOverProperty =
        AvaloniaProperty.Register<ControlGrid, bool>(
            nameof(SelectOver),
            defaultValue: false);

    public static readonly StyledProperty<bool> SnapSelectionProperty =
        AvaloniaProperty.Register<ControlGrid, bool>(
            nameof(SnapSelection),
            defaultValue: false);

    public static readonly StyledProperty<WorkspaceAction> CurrentActionProperty =
         AvaloniaProperty.Register<ControlGrid, WorkspaceAction>(
             nameof(CurrentAction),
             defaultValue: WorkspaceAction.None);

    public static readonly StyledProperty<ObservableCollection<IWorkspaceControl>> SelectedControlsProperty =
        AvaloniaProperty.Register<ControlGrid, ObservableCollection<IWorkspaceControl>>(
            nameof(SelectedControls),
            defaultValue: new ObservableCollection<IWorkspaceControl>());

    public static readonly StyledProperty<double> CellHeightProperty =
        AvaloniaProperty.Register<ControlGrid, double>(nameof(CellHeight), 0d);


    public static readonly RoutedEvent<RoutedEventArgs> ControlsSelectedEvent = 
        RoutedEvent.Register<ControlGrid, RoutedEventArgs>(
            nameof(ControlsSelected), RoutingStrategies.Bubble);

    public static readonly RoutedEvent<RoutedEventArgs> SelectionCompletedEvent =
        RoutedEvent.Register<ControlGrid, RoutedEventArgs>(
            nameof(SelectionCompleted), RoutingStrategies.Bubble);


    public string? TopLeftText
    {
        get => GetValue(TopLeftTextProperty);
        set => SetValue(TopLeftTextProperty, value);
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

    public IEnumerable<IWorkspaceControl> Controls
    {
        get => GetValue(ControlsProperty);
        set => SetValue(ControlsProperty, value);
    }

    public ObservableCollection<IWorkspaceControl> SelectedControls
    {
        get => GetValue(SelectedControlsProperty);
        set => SetValue(SelectedControlsProperty, value);
    }

    public bool DisplayLines
    {
        get => GetValue(DisplayLinesProperty);
        set => SetValue(DisplayLinesProperty, value);
    }

    public bool SelectOver
    {
        get => GetValue(SelectOverProperty);
        set => SetValue(SelectOverProperty, value);
    }

    public bool SnapSelection
    {
        get => GetValue(SnapSelectionProperty);
        set => SetValue(SnapSelectionProperty, value);
    }

    public WorkspaceAction CurrentAction
    {
        get => GetValue(CurrentActionProperty);
        set => SetValue(CurrentActionProperty, value);
    }

    public double CellHeight
    {
        get => GetValue(CellHeightProperty);
        set => SetValue(CellHeightProperty, value);
    }

    public event EventHandler<RoutedEventArgs> ControlsSelected
    {
        add => AddHandler(ControlsSelectedEvent, value);
        remove => RemoveHandler(ControlsSelectedEvent, value);
    }

    public event EventHandler<RoutedEventArgs> SelectionCompleted
    {
        add => AddHandler(SelectionCompletedEvent, value);
        remove => RemoveHandler(SelectionCompletedEvent, value);
    }

    public Rect SelectedArea => _gridLayout!.GetSelectedCoords();

    public ControlGrid()
    {
        InitializeComponent();

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        CellHeight = _gridLayout!.GetCellSize().Height;
    }

    private void ControlGrid_ControlsSelected(object? sender, RoutedEventArgs e)
    {
        if (SelectedControls.Any())
            UpdateGridDisplay();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == EditMode.IsEnabledProperty)
            UpdateSelectionEnabled();

        if (change.Property == CurrentActionProperty)
        {
            if (CurrentAction is WorkspaceAction.Add or WorkspaceAction.None || 
                SelectedControls.Any())
                UpdateGridDisplay();
        }

        if (change.Property == BoundsProperty)
        {
            if (_gridLayout is null)
                return;

            CellHeight = _gridLayout!.GetCellSize().Height;
        }
    }

    private void UpdateGridDisplay()
    {
        if (CurrentAction
                is WorkspaceAction.Add
                or WorkspaceAction.Copy
                or WorkspaceAction.Move)
        {
            PseudoClasses.Set(":selecting", true);

            foreach (var control in Controls)
            {
                if (!control.IsSelected)
                    control.IsMoving = true;

                Log.Debug($"Control {control.ControlId} IsMoving: {control.IsMoving}");
            }

            return;
        }

        PseudoClasses.Set(":selecting", false);

        foreach (var control in Controls)
            control.IsMoving = false;
    }

    private void UpdateSelectionEnabled()
    {
        if (_gridLayout is null)
            return;

        bool editMode = EditMode.GetIsEnabled(this);

        _gridLayout.IsSelectionEnabled = editMode;

        if (!editMode)
        {
            UpdateCurrentSelection();
            HideSelection();
        }
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        DetachGridLayout();
        AttachGridLayout();

        ControlsSelected += ControlGrid_ControlsSelected;
    }

    private void OnUnloaded(object? sender, RoutedEventArgs e)
    {
        DetachGridLayout();
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
    }

    private void AttachGridLayout()
    {
        _gridLayout = this
            .GetVisualDescendants()
            .OfType<GridLayout>()
            .FirstOrDefault()
            ?? throw new InvalidOperationException(
                "Grid layout not found");

        _gridLayout.SelectionStarted += OnSelectionStarted;
        _gridLayout.SelectionChanged += OnSelectionChanged;
        _gridLayout.SelectionCompleted += OnSelectionCompleted;

        UpdateSelectionEnabled();
    }

    private void DetachGridLayout()
    {
        if (_gridLayout is null)
            return;

        _gridLayout.SelectionStarted -= OnSelectionStarted;
        _gridLayout.SelectionChanged -= OnSelectionChanged;
        _gridLayout.SelectionCompleted -= OnSelectionCompleted;

        _gridLayout = null;
    }

    private void OnSelectionStarted(object? sender, EventArgs e)
    {
        UpdateCurrentSelection();
        ShowSelection();
    }

    private void OnSelectionChanged(object? sender, EventArgs e)
    {
        UpdateCurrentSelection();
    }

    private void OnSelectionCompleted(object? sender, EventArgs e)
    {
        UpdateCurrentSelection();
        HideSelection();

        RaiseEvent(new RoutedEventArgs(SelectionCompletedEvent));

        if (SelectedControls.Any())
            RaiseEvent(new RoutedEventArgs(ControlsSelectedEvent));
    }

    private void UpdateCurrentSelection()
    {
        if (_gridLayout is null)
            return;

        var selectedArea = SnapSelection
            ? _gridLayout.GetSelectedCoordsArea()
            : _gridLayout.GetSelectedArea();

        UpdateSelectionRectangle(selectedArea);

        if (SelectOver)
        {
            UpdateSelectedChildren(selectedArea);
        }
    }

    private void UpdateSelectedChildren(Rect selectionArea)
    {
        if (_gridLayout is null || SnapSelection)
            return;

        SelectedControls.Clear();

        // These are normally the ContentPresenters generated by ItemsControl.
        foreach (var container in _gridLayout.Children.OfType<Control>())
        {
            var selectable = ResolveSelectable(container);

            if (selectable is null)
                continue;

            var isSelected =
                selectionArea.Intersects(container.Bounds);

            selectable.IsSelected = isSelected;

            if (isSelected &&
                ResolveHandleControl(container) is { } handleControl &&
                !SelectedControls.Contains(handleControl))
            {
                (SelectedControls).Add(handleControl);
            }
        }
    }

    private static ISelectable? ResolveSelectable(Control container)
    {
        // Handles actual controls placed directly in GridLayout.
        if (container is ISelectable direct)
            return direct;

        // Handles an ItemsControl whose data item implements ISelectable.
        if (container is ContentPresenter
            {
                Content: ISelectable content
            })
        {
            return content;
        }

        // Handles a control created inside a DataTemplate.
        return container
            .GetVisualDescendants()
            .OfType<ISelectable>()
            .FirstOrDefault();
    }

    private static IWorkspaceControl? ResolveHandleControl(Control container)
    {
        if (container is IWorkspaceControl direct)
            return direct;

        if (container is ContentPresenter
            {
                Content: IWorkspaceControl content
            })
        {
            return content;
        }

        return container
            .GetVisualDescendants()
            .OfType<IWorkspaceControl>()
            .FirstOrDefault();
    }

    private void ShowSelection()
    {
        Selection.Transitions = null;
        Selection.Opacity = 1;
    }

    private void HideSelection()
    {
        Selection.Transitions = FadeOutTransitions;
        Selection.Opacity = 0;
    }

    private void UpdateSelectionRectangle(Rect bounds)
    {
        Canvas.SetLeft(Selection, bounds.X);
        Canvas.SetTop(Selection, bounds.Y);

        Selection.Width = bounds.Width;
        Selection.Height = bounds.Height;
    }

    private IBrush HexToBrush(string? hex, double opacity = 1d)
    {
        if (!Color.TryParse(hex, out Color color))
            return ResourceHelper.GetThemeBrush("BorderSubtleBrush");

        opacity = Math.Clamp(opacity, 0, 1);

        var alpha = (byte)Math.Round(opacity * 255d);

        return new SolidColorBrush(
            Color.FromRgb(color.R, color.G, color.B), alpha);
    }
}