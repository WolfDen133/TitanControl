using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using TitanControl.ViewModels.Controls;
using TitanControl.Views.Controls.Toolbar;
using TitanControl.Views.Controls.Toolbar.Button;
using TitanControl.Views.Controls.Toolbar.Buttons;

namespace TitanControl;

public partial class Toolbar : UserControl
{
    public ToolbarModel Model
    {
        get
        {
            if (DataContext is not ToolbarModel m)
                throw new InvalidOperationException($"Could not find valid data context for {nameof(Toolbar)}.");

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

    public void DoResize(int height)
    {
        Height = height;

        InvalidateMeasure();
        InvalidateArrange();
    }

    public int CalculateHeight(int windowWidth)
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