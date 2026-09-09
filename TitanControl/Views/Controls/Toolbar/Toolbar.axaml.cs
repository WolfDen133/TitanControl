using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using System;
using System.Diagnostics;
using System.Linq;
using TitanControl.Events.Control;
using TitanControl.Logging;
using TitanControl.ViewModels.Controls.Toolbar;
using TitanControl.Views.Controls.Layout.Grid;
using TitanControl.Views.Controls.Toolbar;
using TitanControl.Views.Controls.Toolbar.Button;
using TitanControl.Views.Controls.Toolbar.Buttons;
using TitanControl.Views.State;

namespace TitanControl.Views.Controls.Toolbar;

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

    protected override void OnPropertyChanged(
        AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == EditMode.IsEnabledProperty)
        {
            Log.Debug(
                $"Toolbar edit mode: {EditMode.GetIsEnabled(this)}");
        }
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
}