using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Humanizer;
using ShimSkiaSharp.Editing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TitanControl.Events.Control;
using TitanControl.Logging;
using TitanControl.ViewModel;
using TitanControl.ViewModels.Controls;
using TitanControl.ViewModels.Page;
using TitanControl.Views.Controls.Layout.Grid;
using TitanControl.Views.Controls.Toolbar;
using TitanControl.Views.Controls.Toolbar.Button;
using TitanControl.Views.Pages;
using static System.Net.Mime.MediaTypeNames;

namespace TitanControl.Views;

[PseudoClasses(":loading")]
public partial class MainWindow : Window
{
    private const string LoggingCategory = "MainWindow";

    public static readonly TimeSpan PageTransitonDuration =
          TimeSpan.FromMilliseconds(300);

    private TranslateTransform PageTransform =>
        (TranslateTransform)PageContainer.RenderTransform!;

    private Dictionary<PageId, BasePage> _pages = new();

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

        PseudoClasses.Set(":loading", true);

        AddHandler(GridLayout.GridDoubleClickedEvent, OnGrid_DoubleClicked, RoutingStrategies.Bubble);
        AddHandler(Toolstrip.ToolButtonPressedEvent, OnToolButtonClicked, RoutingStrategies.Bubble);
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        if (Design.IsDesignMode)
            return;

        ToolbarContainer.Height = 0;

        SetPagePositionImmediately(Model.CurrentPage != PageId.None);

        Model.RequestSplash += (s, isVisible) => SetSplashVisible(isVisible);
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);

        if (Design.IsDesignMode || !Model.EditMode)
            return;

        double height = PART_Toolbar.CalculateHeight(e.NewSize.Width);

        SetToolbarHeightImmediately(height);
        PART_Toolbar.DoResize(height);

        if (Model.CurrentPage == PageId.None)
            SetPagePositionImmediately(false);
    }

    public async Task InitializationCompleted()
    {
        Dispatcher.Post(ScanPages, DispatcherPriority.Loaded);

        // Cheeky but feels smoother
        await Task.Delay(200);

        SetSplashVisible(false);
    }

    public void SetSplashVisible(bool isVisible = true)
    {
        PART_Splash.Opacity = isVisible ? 1 : 0;
        PART_Splash.IsHitTestVisible = isVisible;

        PART_Content.IsHitTestVisible = !isVisible;
    }

    private async void OnToolButtonClicked(object? sender, ToolButtonPressedEventArgs e)
    {
        await Model.OnToolButtonClicked(e.ButtonId, e.ButtonAction);
    }

    private void OnGrid_DoubleClicked(object? sender, GridDoubleClickedEventArgs e)
    {
        Model.EnableEditMode(!Model.EditMode);
        HandleToolbarVisibility(Model.EditMode);
        e.Handled = true;
    }

    private async Task OnPageRequestOpen(IPageModel pageModel)
    {
        if (!_pages.TryGetValue(pageModel.Id, out BasePage? page))
        {
            var ex = new InvalidOperationException("Page not found");
            Log.Error(ex, $"Cound not find page {pageModel.Id} to close.", LoggingCategory);
            throw ex;
        }

        await OpenPage(page);
    }

    private async Task OnPageRequestClose(IPageModel pageModel)
    {
        if (!_pages.TryGetValue(pageModel.Id, out BasePage? page))
        {
            var ex = new InvalidOperationException("Page not found");
            Log.Error(ex, $"Cound not find page {pageModel.Id} to close.", LoggingCategory);
            throw ex;
        }

        await ClosePage(page);
    }

    private async Task OpenPage(BasePage page)
    {
        page.State = PageState.Opening;
        page.IsActive = true;

        SetPagePositionImmediately(false, page.Dock);

        page.IsVisible = true;

        HandlePanelVisibility(PageContainer, true);

        Dispatcher.UIThread.Post(() =>
        {
            PageTransform.X = 0;
            PageTransform.Y = 0;
        }, DispatcherPriority.Render);

        await Task.Delay(PageTransitonDuration);

        if (page.IsActive)
            page.State = PageState.Open;
    }

    private async Task ClosePage(BasePage page)
    {
        var dockPosition = GetHiddenOffset(page.Dock);

        page.IsActive = false;
        page.State = PageState.Closing;

        PageTransform.X = dockPosition.X;
        PageTransform.Y = dockPosition.Y;

        HandlePanelVisibility(PageContainer, false);

        await Task.Delay(PageTransitonDuration);

        if (!page.IsActive)
        {
            page.IsVisible = false;
            page.State = PageState.Closed;
        }

        Log.Debug($"Closing page {page.Id}");
    }

    private void ScanPages()
    {
        var pages = this.GetVisualDescendants().OfType<BasePage>();
        var count = pages.ToList().Count;

        if (count < 1)
        {
            var ex = new InvalidOperationException("No pages found");
            Log.Error(ex, "Cound not find any pages to assign models to.", LoggingCategory);
            throw ex;
        }

        Log.Debug($"Found {count} pages.", LoggingCategory);

        foreach (var page in pages)
        {
            if (!Model.PageModels.TryGetValue(page.Id, out IPageModel? model))
            {
                var ex = new InvalidOperationException("No page model found");
                Log.Error(ex, $"Cound not find page model for {page.Id}.", LoggingCategory);
                throw ex;
            }

            model.RequestOpen += OnPageRequestOpen;
            model.RequestClose += OnPageRequestClose;

            page.DataContext = model;

            page.IsActive = false;
            page.IsVisible = false;

            if (!_pages.TryAdd(page.Id, page))
            {
                throw new InvalidOperationException(
                    $"Multiple views were registered for page {page.Id}.");
            }
        }
    }
   

    private void HandleToolbarVisibility(bool visible)
    {
        HandlePanelVisibility(ToolbarContainer, visible);

        if (visible)
        {
            double height = PART_Toolbar.CalculateHeight((int)Bounds.Width);

            ToolbarContainer.Height = height;
            PART_Toolbar.DoResize(height);
        }
        else
        {
            ToolbarContainer.Height = 0;
            Model.WorkspaceModel.ClearSelection();
        }
    }

    private void HandlePanelVisibility(Panel panel, bool visible)
    {
        if (!panel.IsVisible)
            panel.IsVisible = true;

        panel.Opacity = visible ? 1 : 0;
        panel.IsHitTestVisible = visible;
    }

    private void SetPagePositionImmediately(bool visible, Dock dock = Dock.Top)
    {
        var transitions = PageTransform.Transitions;
       
        PageTransform.Transitions = null;

        var dockPosition = GetHiddenOffset(dock);

        PageTransform.Y = visible ? 0 : dockPosition.Y;
        PageTransform.X = visible ? 0 : dockPosition.X;

        PageContainer.IsHitTestVisible = visible;
        PageTransform.Transitions = transitions;

        SetPageBorders(dock);
    }

    private void SetPageBorders(Dock dock = Dock.Top)
    {
        PageBorder.BorderThickness = GetDockThickness(dock);
        PageBorder.CornerRadius = GetDockRadius(dock);
        PageBorder.Margin = GetDockThickness(dock, 5d);
        PageClip.CornerRadius = GetDockRadius(dock, 8d);
    }

    private void SetToolbarHeightImmediately(double height)
    {
        var transitions = ToolbarContainer.Transitions;

        ToolbarContainer.Transitions = null;
        ToolbarContainer.Height = height;
        ToolbarContainer.Transitions = transitions;
    }

    private static Thickness GetDockThickness(Dock dock, double thickness = 2d)
    {
        return dock switch
        {
            Dock.Top => new(thickness, 0d, thickness, thickness),
            Dock.Bottom => new(thickness, thickness, thickness, 0d),
            Dock.Left => new(0d, thickness, thickness, thickness),
            Dock.Right => new(thickness, thickness, 0d, thickness),
            _ => new(thickness, thickness, thickness, thickness)
        };
    }

    private static CornerRadius GetDockRadius(Dock dock, double radius = 10d)
    {
        return dock switch
        {
            Dock.Top => new(0d, 0d, radius, radius),
            Dock.Bottom => new(radius, radius, 0d, 0d),
            Dock.Left => new(0d, radius, 0d, radius),
            Dock.Right => new(radius, 0d, radius, 0d),
            _ => new(radius, radius, radius, radius)
        };
    }

    private Point GetHiddenOffset(Dock dock)
    {
        return dock switch
        {
            Dock.Top => new(0d, -PageAnchor.Bounds.Height),
            Dock.Bottom => new(0d, +PageAnchor.Bounds.Height),
            Dock.Left => new(-PageAnchor.Bounds.Width, 0d),
            Dock.Right => new(+PageAnchor.Bounds.Width, 0d),
            _ => new(0, 0)
        };
    }
}