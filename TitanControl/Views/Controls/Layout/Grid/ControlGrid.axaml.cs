using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using System;
using System.Collections.Generic;
using System.Linq;
using TitanControl.Views.Controls.Handle;
using TitanControl.Views.State;

namespace TitanControl.Views.Controls.Layout.Grid
{
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

        public static readonly StyledProperty<int> RowsProperty =
            AvaloniaProperty.Register<GridLayout, int>(nameof(Rows), 12);

        public static readonly StyledProperty<int> ColumnsProperty =
            AvaloniaProperty.Register<GridLayout, int>(nameof(Columns), 12);

        public static readonly StyledProperty<bool> DisplayLinesProperty =
            AvaloniaProperty.Register<GridLayout, bool>(nameof(DisplayLines), true);

        public static readonly StyledProperty<IEnumerable<IHandleControl>?> ControlsProperty =
        AvaloniaProperty.Register<ControlGrid, IEnumerable<IHandleControl>?>(
            nameof(Controls));


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

        public IEnumerable<IHandleControl>? Controls
        {
            get => GetValue(ControlsProperty);
            set => SetValue(ControlsProperty, value);
        }

        public bool DisplayLines
        {
            get => GetValue(DisplayLinesProperty);
            set => SetValue(DisplayLinesProperty, value);
        }

        public ControlGrid()
        {
            InitializeComponent();

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private void OnLoaded(object? sender, RoutedEventArgs e)
        {
            _gridLayout = this
                .GetVisualDescendants()
                .OfType<GridLayout>()
                .SingleOrDefault();

            if (_gridLayout is null)
                return; // Or throw while developing.

            EditMode.IsEnabledProperty.Changed.AddClassHandler<ControlGrid>((s, e) =>
            {
                if ((bool)e.NewValue! == true)
                    AddGridHandlers();
                else
                    RemoveGridHandlers();
            });
        }

        private void AddGridHandlers()
        {
            if (_gridLayout is null)
                return;

            _gridLayout.PointerPressed += OnGridPointerPressed;
            _gridLayout.PointerMoved += OnGridPointerMoved;
            _gridLayout.PointerReleased += OnGridPointerReleased;
        }

        private void RemoveGridHandlers()
        {
            if (_gridLayout is null)
                return;

            _gridLayout.PointerPressed -= OnGridPointerPressed;
            _gridLayout.PointerMoved -= OnGridPointerMoved;
            _gridLayout.PointerReleased -= OnGridPointerReleased;
        }

        private void OnUnloaded(object? sender, RoutedEventArgs e)
        {
            if (_gridLayout is null)
                return;

            RemoveGridHandlers();

            _gridLayout = null;
        }

        private void OnGridPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (_gridLayout is null)
                return;

            var point = e.GetCurrentPoint(this);

            SnapSelection = point.Properties.PointerUpdateKind != PointerUpdateKind.LeftButtonPressed;

            UpdateSelection(
                !SnapSelection
                    ? _gridLayout.GetSelectedArea()
                    : _gridLayout.GetSelectedCoordsArea());

            ShowSelection();
        }

        private void OnGridPointerMoved(object? sender, PointerEventArgs e)
        {
            if (_gridLayout is null)
                return;

            UpdateSelection(
                !SnapSelection
                    ? _gridLayout.GetSelectedArea()
                    : _gridLayout.GetSelectedCoordsArea());
        }

        private void OnGridPointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            if (_gridLayout is null)
                return;

            HideSelection();
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

        private void UpdateSelection(Rect bounds)
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
    }
}