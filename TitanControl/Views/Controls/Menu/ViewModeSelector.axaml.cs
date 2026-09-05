using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using TitanControl.ViewModels;

namespace TitanControl.Views.Controls.Menu;

public class ViewModeSelector : TemplatedControl
{
    public static readonly StyledProperty<ViewMode> ViewModeProperty =
        AvaloniaProperty.Register<ViewModeSelector, ViewMode>(
            nameof(ViewMode),
            ViewMode.Grid,
            defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    public ViewMode ViewMode
    {
        get => GetValue(ViewModeProperty);
        set => SetValue(ViewModeProperty, value);
    }

    private ToggleButton? _gridButton;
    private ToggleButton? _listButton;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (_gridButton is not null)
            _gridButton.Click -= GridButton_Click;

        if (_listButton is not null)
            _listButton.Click -= ListButton_Click;

        _gridButton = e.NameScope.Find<ToggleButton>("PART_GridButton");
        _listButton = e.NameScope.Find<ToggleButton>("PART_ListButton");

        if (_gridButton is not null)
            _gridButton.Click += GridButton_Click;

        if (_listButton is not null)
            _listButton.Click += ListButton_Click;

        UpdateSelection();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ViewModeProperty)
            UpdateSelection();
    }

    private void GridButton_Click(object? sender, RoutedEventArgs e)
    {
        ViewMode = ViewMode.Grid;
    }

    private void ListButton_Click(object? sender, RoutedEventArgs e)
    {
        ViewMode = ViewMode.List;
    }

    private void UpdateSelection()
    {
        if (_gridButton is not null)
            _gridButton.IsChecked = ViewMode == ViewMode.Grid;

        if (_listButton is not null)
            _listButton.IsChecked = ViewMode == ViewMode.List;
    }
}

public enum ViewMode
{
    Grid,
    List
}