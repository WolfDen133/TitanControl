using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using System;
using TitanControl.Logging;
using TitanControl.Views.Controls.Layout;

namespace TitanControl.Views.Controls.Toolbar;

public class InfoPanel : TemplatedControl
{
    private Grid? _layout;
    private Grid? _details;
    private Grid? _brandLayout;

    private Border? _brand;
    private Border? _detailsContainer;
    private Border? _workspace;
    private Border? _session;
    private Border? _logo;

    private Control? _brandText;
    private Control? _compactVersion;
    private Control? _workspaceLabel;
    private Control? _sessionLabel;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _layout = Find<Grid>(e, "PART_Layout");
        _details = Find<Grid>(e, "PART_Details");
        _brandLayout = Find<Grid>(e, "PART_BrandLayout");

        _brand = Find<Border>(e, "PART_Brand");
        _detailsContainer = Find<Border>(e, "PART_DetailsContainer");
        _workspace = Find<Border>(e, "PART_Workspace");
        _session = Find<Border>(e, "PART_Session");
        _logo = Find<Border>(e, "PART_Logo");

        _brandText = Find<Control>(e, "PART_BrandText");
        _compactVersion = Find<Control>(e, "PART_CompactVersion");

        _workspaceLabel = Find<Control>(e, "PART_WorkspaceLabel");
        _sessionLabel = Find<Control>(e, "PART_SessionLabel");

        Classes.CollectionChanged += (_, _) => UpdateFlow();

        UpdateFlow();
    }

    private static T Find<T>(
        TemplateAppliedEventArgs e,
        string name) where T : Control
    {
        return e.NameScope.Find<T>(name)
            ?? throw new InvalidOperationException(
                $"InfoPanel template is missing {name}.");
    }

    private void UpdateFlow()
    {
        // Breakpoint classes can change before the template is applied.
        if (_layout is null ||
            _details is null ||
            _brandLayout is null ||
            _brand is null ||
            _detailsContainer is null ||
            _workspace is null ||
            _session is null ||
            _logo is null ||
            _brandText is null ||
            _compactVersion is null ||
            _workspaceLabel is null ||
            _sessionLabel is null)
        {
            return;
        }

        Sizing size = Classes.Contains("compact")
            ? Sizing.Compact
            : Classes.Contains("small")
                ? Sizing.Small
                : Sizing.Normal;

        bool normal = size == Sizing.Normal;
        bool small = size == Sizing.Small;
        bool compact = size == Sizing.Compact;

        // OUTER LAYOUT
        // Normal: branding above details.
        // Small/compact: branding beside details.
        // PART_Layout always has 2 columns and 2 rows.
        _layout.ColumnDefinitions[0].Width = normal
            ? new GridLength(1, GridUnitType.Star)
            : GridLength.Auto;

        _layout.ColumnDefinitions[1].Width = normal
            ? new GridLength(0)
            : new GridLength(1, GridUnitType.Star);

        _layout.RowDefinitions[0].Height = normal
            ? new GridLength(1.2, GridUnitType.Star)
            : new GridLength(1, GridUnitType.Star);

        _layout.RowDefinitions[1].Height = normal
            ? new GridLength(1, GridUnitType.Star)
            : new GridLength(0);

        Grid.SetColumn(_brand, 0);
        Grid.SetRow(_brand, 0);

        Grid.SetColumn(_detailsContainer, normal ? 0 : 1);
        Grid.SetRow(_detailsContainer, normal ? 1 : 0);

        _brand.BorderThickness = normal
            ? new Thickness(0, 0, 0, 1)
            : new Thickness(0, 0, 1, 0);

        // Keep the logo inside a reasonably sized slot.
        _logo.Width = !normal ? 44 : double.NaN;

        // WORKSPACE / SESSION
        // Small: stack details.
        // Normal: two columns.
        // Compact: two inline columns.
        // DETAILS
        _details.ColumnDefinitions = small
            ? new ColumnDefinitions("*")
            : new ColumnDefinitions("*,*");

        _details.RowDefinitions = small
            ? new RowDefinitions("*,*")
            : new RowDefinitions("*");

        Grid.SetColumn(_workspace, 0);
        Grid.SetRow(_workspace, 0);

        Grid.SetColumn(_session, small ? 0 : 1);
        Grid.SetRow(_session, small ? 1 : 0);

        _workspace.BorderThickness = small
            ? new Thickness(0, 0, 0, 1)
            : new Thickness(0, 0, 1, 0);

        FluidPadding.SetReferencePadding(
            _workspace,
            compact ? new Thickness(4, 0) : new Thickness(8, 0));

        FluidPadding.SetReferencePadding(
            _session,
            compact ? new Thickness(4, 0) : new Thickness(8, 0));

        _brandLayout.ColumnDefinitions = compact
            ? new ColumnDefinitions("Auto")
            : new ColumnDefinitions("Auto,*");

        _brandLayout.RowDefinitions = compact
            ? new RowDefinitions("*,Auto")
            : new RowDefinitions("*");

        _brandLayout.HorizontalAlignment = compact
            ? Avalonia.Layout.HorizontalAlignment.Center
            : Avalonia.Layout.HorizontalAlignment.Left;

        Grid.SetColumn(_logo, 0);
        Grid.SetRow(_logo, 0);

        Grid.SetColumn(_brandText, compact ? 0 : 1);
        Grid.SetRow(_brandText, 0);

        Grid.SetColumn(_compactVersion, 0);
        Grid.SetRow(_compactVersion, compact ? 1 : 0);

        _brandText.IsVisible = !compact;
        _compactVersion.IsVisible = compact;

        _layout.InvalidateMeasure();
        _details.InvalidateMeasure();

        InvalidateMeasure();
    }
}