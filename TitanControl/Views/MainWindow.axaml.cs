using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using ShimSkiaSharp.Editing;
using System;
using System.Threading.Tasks;
using TitanControl.Events.Control;
using TitanControl.Logging;
using TitanControl.ViewModel;
using TitanControl.Views.Controls.Layout.Grid;

namespace TitanControl.Views;

public partial class MainWindow : Window
{
    private TranslateTransform PageTransform =>
        (TranslateTransform)PageContainer.RenderTransform!;

    private Transitions? _toolbarTransitions;

    public MainWindowModel Model
    {
        get
        {
            if (DataContext is not MainWindowModel m)
                throw new InvalidOperationException($"Could not find valid data context for {nameof(MainWindow)}.");

            return m;
        }
    }


    public MainWindow()
    {
        InitializeComponent();

        AddHandler(GridLayout.GridDoubleClickedEvent, OnGridDoubleClicked);
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        if (Design.IsDesignMode)
            return;

        Model.PropertyChanged += Model_PropertyChanged;

        ToolbarContainer.Height = 0;

        _toolbarTransitions = ToolbarContainer.Transitions;

        SetPagePositionImmediately(
            visible:
                Model.CurrentPage !=
                ViewModels.Page.PageId.Workspace);
    }

    private void SetPagePositionImmediately(bool visible)
    {
        var transitions = PageTransform.Transitions;

        PageTransform.Transitions = null;

        PageTransform.Y =
            visible
                ? 0
                : -PageAnchor.Bounds.Height;

        PageContainer.IsHitTestVisible =
            visible;

        PageTransform.Transitions =
            transitions;
    }

    private void Model_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(Model.CurrentPage))
            return;

        Log.Debug($"Changing to {Model.CurrentPage}");

        if (Model.CurrentPage != ViewModels.Page.PageId.Workspace)
            HandlePageVisibility(true);
        else
            HandlePageVisibility(false);
    }

    private void OnGridDoubleClicked(object? sender, GridDoubleClickedEventArgs e)
    {
        Model.EnableEditMode(!Model.EditMode);
        HandleToolbarVisibility(Model.EditMode);
        e.Handled = true;
    }

    private void HandlePageVisibility(bool visible)
    {
        HandlePanelVisibility(PageContainer, visible);
        PageContainer.IsHitTestVisible = visible;

        PageTransform.Y =
            visible
                ? 0
                : -PageAnchor.Bounds.Height;
    }

    private void HandleToolbarVisibility(bool visible)
    {
        HandlePanelVisibility(ToolbarContainer, visible);

        if (visible)
        {
            int height = PART_Toolbar.CalculateHeight((int)Bounds.Width);

            ToolbarContainer.Height = height;
            PART_Toolbar.DoResize(height);
        }
        else 
            ToolbarContainer.Height = 0;
    }

    private void HandlePanelVisibility(Panel panel, bool visible)
    {
        if (!panel.IsVisible)
            panel.IsVisible = true;

        if (visible)
            panel.Opacity = 1;
        else
            panel.Opacity = 0;
    }

    private void SetToolbarHeightImmediately(int height)
    {
        var transitions = ToolbarContainer.Transitions;

        ToolbarContainer.Transitions = null;
        ToolbarContainer.Height = height;
        ToolbarContainer.Transitions = transitions;
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);

        if (Design.IsDesignMode || !Model.EditMode)
            return;

        int height = PART_Toolbar.CalculateHeight((int)e.NewSize.Width);

        SetToolbarHeightImmediately(height);
        PART_Toolbar.DoResize(height);

        if (Model.CurrentPage == ViewModels.Page.PageId.Workspace)
        {
            SetPagePositionImmediately(false);
        }
    }
}