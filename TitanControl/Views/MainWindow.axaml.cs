using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
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
using TitanControl.Views.Controls.Toolbar.Button;
using TitanControl.Views.Pages;
using static System.Net.Mime.MediaTypeNames;

namespace TitanControl.Views;

public partial class MainWindow : Window
{
    private const string LoggingCategory = "MainWindow";

    public static readonly TimeSpan PageTransitonDuration =
          TimeSpan.FromMilliseconds(300);

    private TranslateTransform PageTransform =>
        (TranslateTransform)PageContainer.RenderTransform!;

    private Transitions? _toolbarTransitions;
    private Dictionary<PageId, BasePage> _pages = new();
    private readonly List<PageId> _pageHistory = new();

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
        AddHandler(GridLayout.GridDoubleClickedEvent, OnGrid_DoubleClicked);
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        if (Design.IsDesignMode)
            return;

        Dispatcher.Post(ScanPages, DispatcherPriority.Loaded);

        ToolbarContainer.Height = 0;

        Model.ToolbarModel.ButtonClicked += ToolbarModel_ButtonClicked;

        _toolbarTransitions = ToolbarContainer.Transitions;

        SetPagePositionImmediately(Model.CurrentPage != ViewModels.Page.PageId.None);
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);

        Model.ToolbarModel.ButtonClicked -= ToolbarModel_ButtonClicked;
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);

        if (Design.IsDesignMode || !Model.EditMode)
            return;

        int height = PART_Toolbar.CalculateHeight((int)e.NewSize.Width);

        SetToolbarHeightImmediately(height);
        PART_Toolbar.DoResize(height);

        if (Model.CurrentPage == ViewModels.Page.PageId.None)
        {
            SetPagePositionImmediately(false);
        }
    }

    private async void ToolbarModel_ButtonClicked(object? sender, ToolButtonPressedEventArgs e)
    {
        PageId page = e.ButtonId switch
        { 
            ButtonId.Sessions => PageId.Session,
            ButtonId.Assign => PageId.HandleBrowser,
            _ => PageId.None
        };

        if (page == PageId.None)
            return;

        switch (e.ButtonAction)
        {
            case ButtonAction.ToggleDown:
                await NavigateTo(page);
                break;

            case ButtonAction.ToggleUp:
                await ClosePageNavigation(page);
                break;
        }
    }

    private void OnGrid_DoubleClicked(object? sender, GridDoubleClickedEventArgs e)
    {
        Model.EnableEditMode(!Model.EditMode);
        HandleToolbarVisibility(Model.EditMode);
        e.Handled = true;
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

            page.IsActive = false;
            page.IsVisible = false;
            page.DataContext = model;

            if (!_pages.TryAdd(page.Id, page))
            {
                throw new InvalidOperationException(
                    $"Multiple views were registered for page {page.Id}.");
            }
        }
    }

    private async Task NavigateTo(PageId id)
    {
        if (!_pages.TryGetValue(id, out var page))
            throw new InvalidOperationException($"No page found for {id}.");

        // Already on top.
        if (_pageHistory.Count > 0 &&
            _pageHistory[^1] == id)
            return;

        // Close the currently displayed page.
        if (_pageHistory.Count > 0)
        {
            var currentId = _pageHistory[^1];

            if (_pages.TryGetValue(currentId, out var currentPage))
                await ClosePage(currentPage);
        }

        // Avoid duplicate history entries.
        _pageHistory.Remove(id);
        _pageHistory.Add(id);

        await OpenPage(page);
    }

    private async Task ClosePageNavigation(PageId id)
    {
        int index = _pageHistory.IndexOf(id);

        if (index < 0)
            return;

        bool isCurrentPage =
            index == _pageHistory.Count - 1;

        // Remove THIS page, regardless of where it is.
        _pageHistory.RemoveAt(index);

        // It wasn't the visible page.
        // Nothing visually needs to change.
        if (!isCurrentPage)
            return;

        if (_pages.TryGetValue(id, out var page))
            await ClosePage(page);

        // Reveal whatever was underneath.
        if (_pageHistory.Count > 0)
        {
            var previousId = _pageHistory[^1];

            if (_pages.TryGetValue(previousId, out var previousPage))
                await OpenPage(previousPage);
        }
    }

    private async Task NavigateBack()
    {
        if (_pageHistory.Count == 0)
            return;

        // Current page is always the last item.
        PageId currentId = _pageHistory[^1];

        _pageHistory.RemoveAt(_pageHistory.Count - 1);

        if (_pages.TryGetValue(currentId, out var currentPage))
            await ClosePage(currentPage);

        // Nothing underneath -> back to workspace.
        if (_pageHistory.Count == 0)
            return;

        PageId previousId = _pageHistory[^1];

        if (_pages.TryGetValue(previousId, out var previousPage))
            await OpenPage(previousPage);
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
        PageContainer.IsHitTestVisible = false;

        await Task.Delay(PageTransitonDuration);

        if (!page.IsActive)
        {
            page.IsVisible = false;
            page.State = PageState.Closed;
        }

        Log.Debug($"Closing page {page.Id}");
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

        panel.Opacity = visible ? 1 : 0;
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

    private void SetToolbarHeightImmediately(int height)
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