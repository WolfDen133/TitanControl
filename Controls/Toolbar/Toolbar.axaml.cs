using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;
using System.Diagnostics;
using TitanControl.Controls.Models;
using TitanControl.Controls.Toolbar;

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

        int width = (int)(windowWidth / 4.9f);

        PART_InfoPanel.Width = width;

        InvalidateMeasure();
        InvalidateArrange();
    }

    private int CalculateHeight(int windowWidth)
    {
        float displayMultiplier = Math.Min((float)(windowWidth / 1500f), 1);
        float height = 127f * displayMultiplier;
        height = Math.Min(height, (windowWidth / 2 - Math.Min(400, (windowWidth / 4.85f) / 2) - 30) / Toolstrip.MaxPerPage);

        return (int)height;
    }
}