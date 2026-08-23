using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using TitanControl.Controls.Models;
using TitanControl.Controls.Toolbar;
using TitanControl.Controls.Toolbar.Buttons;
using TitanControl.Services.Session;

namespace TitanControl;

public partial class Toolbar : UserControl
{
    public ToolbarModel Model
    {
        get
        {
            if (DataContext is not ToolbarModel m)
                throw new InvalidOperationException($"Could not find valid data context for {nameof(MainWindow)}.");

            return m;
        }
    }

    public Toolbar()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
    }

    public void DoResize(int windowWidth)
    {
        int height = CalculateHeight(windowWidth);

        Height = height;

        InvalidateMeasure();
        InvalidateArrange();
    }

    private int CalculateHeight(int windowWidth)
    {
        float displayMultiplier = Math.Min((float)(windowWidth / 1500f), 1);
        float height = 130f * displayMultiplier;
        height = Math.Min(height, (windowWidth / 2 - Math.Min(138, (windowWidth / 4.85f) / 2) - 30) / Toolstrip.MaxPerPage);

        return (int)height;
    }

    private void ToolbarButton_OnClick(object? sender, ToolbarButton.ButtonAction e)
    {
        if (sender is ToolbarButton button && !Design.IsDesignMode)
            Model.OnButtonClicked((ButtonId)button.ID, e);
    }
}