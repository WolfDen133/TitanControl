using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;
using System.Diagnostics;
using TitanControl.Controls.Models;
using TitanControl.Controls.Toolbar;
using TitanControl.Controls.Toolbar.Buttons;
using TitanControl.Services.Session;
using TitanControl.WebAPI;

namespace TitanControl;

public partial class Toolbar : UserControl
{
    public Toolbar()
    {
        InitializeComponent();
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

    public void UpdateSessionStatus(SessionConnectionState state)
    {
        PART_InfoPane.UpdateStatus(state);
    }

    private void ToolbarButton_OnClick(object? sender, ToolbarButton.ButtonAction e)
    {
        if (sender is ToolbarButton button)

    }
}