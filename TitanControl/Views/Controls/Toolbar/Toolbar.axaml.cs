using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using System;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;
using TitanControl.Events.Control;
using TitanControl.Logging;
using TitanControl.ViewModels.Controls.Toolbar;
using TitanControl.Views.Controls.Layout.Grid;
using TitanControl.Views.Controls.Toolbar;
using TitanControl.Views.Controls.Toolbar.Button;
using TitanControl.Views.Controls.Toolbar.Buttons;
using TitanControl.Views.State;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TitanControl.Views.Controls.Toolbar
{
    public partial class Toolbar : UserControl
    {
        private Toolstrip? _systemToolstrip;
        private Toolstrip? _contextToolstrip;

        private Sizing? _appliedSize;

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

            _appliedSize = null;
        }

        public void ContextItems_Loaded(object? sender, RoutedEventArgs e)
        {
            _contextToolstrip = PART_ContextItems.ItemsPanelRoot as Toolstrip;
        }

        public void SystemItems_Loaded(object? sender, RoutedEventArgs e)
        {
            _systemToolstrip = PART_SystemItems.ItemsPanelRoot as Toolstrip;
        }

        public void DoResize(double height)
        {
            if (_contextToolstrip is null || _systemToolstrip is null)
                return;

            Sizing size = Classes.Contains("compact")
                ? Sizing.Compact
                : Classes.Contains("small")
                    ? Sizing.Small
                    : Sizing.Normal;

            Height = height;

            if (_appliedSize != size)
            {
                bool isCompact = Bounds.Width <= 1200;

                PART_InfoPane.Classes.Set("small", Bounds.Width <= 1500 && !isCompact);
                PART_InfoPane.Classes.Set("compact", isCompact);

                _systemToolstrip.Classes.Set("compact", isCompact);
                _contextToolstrip.Classes.Set("compact", isCompact);

                PART_InfoContainer.MaxWidth = isCompact ? 380 : double.PositiveInfinity;
            }

            InvalidateMeasure();
            InvalidateArrange();
        }

        public double CalculateHeight(double windowWidth)
        {
            if (!double.IsFinite(windowWidth) || windowWidth <= 0)
                return 0;

            double scale = Math.Min(windowWidth / 1500d, 1d);
            double preferredHeight = 130d * scale;

            double reservedWidth = Math.Min(
                138d,
                windowWidth / 9.7d);

            double availableWidth =
                windowWidth / 2d - reservedWidth - 30d;

            double widthLimitedHeight =
                Math.Max(0d, availableWidth / Toolstrip.MaxPerPage);

            return Math.Min(preferredHeight, widthLimitedHeight);
        }
    }

    public enum Sizing
    {
        Normal,
        Small,
        Compact
    }
}
