using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Raw;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TitanControl.Controls.Models;
using TitanControl.Controls.Toolbar.Buttons;
using TitanControl.Logging;

namespace TitanControl.Controls.Toolbar
{
    public class Toolstrip : StackPanel
    {
        private const string LogCategory = "Toolstrip";

        private int _current = -1;

        public static int MaxPerPage { get; } = 6;
        public ObservableCollection<ToolbarButton> MenuTree { get; set; } = new();
        public bool Exclusive { get; set; }

        public int Current => _current;

        public Toolstrip()
        {
            FlowDirection = Avalonia.Media.FlowDirection.LeftToRight;
            Orientation = Avalonia.Layout.Orientation.Horizontal;
            Margin = new Thickness(4);
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);

            InitializeButtons();
            LoadDefaultPage();
        }

        private void InitializeButtons()
        {
            foreach (ToolbarButton button in MenuTree.Values)
            {
                button.IsVisible = false;

                button.PointerReleased += OnButtonPointerReleased;
            }
        }

        private void OnButtonPointerReleased(
            object? sender,
            PointerReleasedEventArgs e)
        {
            if (!Exclusive || sender is not ToolbarButton selectedButton)
                return;

            foreach (ToolbarButton button in _menuTree.Values)
            {
                if (!ReferenceEquals(button, selectedButton))
                    button.ReleaseToggle();
            }
        }

        /* TODO
        protected override void OnDoubleClick(EventArgs e)
        {
            base.OnDoubleClick(e);

            WorkspaceView.Instance?.EnableEditMode(false);
        }*/

        protected virtual void LoadDefaultPage()
        {
            ShowPage(-1, false);
        }

        public void LoadPage(int index)
        {
            if (index == 0)
            {
                LoadDefaultPage();
                return;
            }

            if (!_menuTree.TryGetValue(index, out ToolbarButton? pageButton))
            {
                Log.Warning(
                    $"Unable to load toolbar page {index}: " +
                    "the page does not exist.",
                    LogCategory);

                return;
            }

            if (pageButton.ID == -1)
            {
                LoadDefaultPage();
                return;
            }

            if (pageButton.Children is null)
            {
                Log.Warning(
                    $"Unable to load toolbar page {index}: " +
                    "the page has no child buttons.",
                    LogCategory);

                return;
            }

            ShowPage(index, includeBackButton: true);
        }

        private void ShowPage(int index, bool includeBackButton)
        {
            /*
             * Hide everything first. The controls remain attached and retain
             * their loaded SVG state.
             */
            foreach (ToolbarButton button in _menuTree.Values)
                button.IsVisible = false;

            if (includeBackButton &&
                _menuTree.TryGetValue(-1, out ToolbarButton? backButton))
            {
                backButton.IsVisible = true;
            }

            if (index == -1)
            {
                ShowDefaultButtons();
                _current = -1;

                InvalidateMeasure();
                InvalidateArrange();
                return;
            }

            ToolbarButton page = _menuTree[index];

            foreach (int childId in page.Children!)
            {
                if (_menuTree.TryGetValue(
                        childId,
                        out ToolbarButton? childButton))
                {
                    childButton.IsVisible = true;
                }
                else
                {
                    Log.Warning(
                        $"Toolbar page {index} references missing " +
                        $"button {childId}.",
                        LogCategory);
                }
            }

            _current = index;

            InvalidateMeasure();
            InvalidateArrange();
        }

        protected virtual void ShowDefaultButtons()
        {
            SetButtonVisible(1);
            SetButtonVisible(2);
            SetButtonVisible(3);
            SetButtonVisible(4);
            SetButtonVisible(5);
            SetButtonVisible(6);
        }

        protected void SetButtonVisible(int buttonId, bool visible = true)
        {
            if (_menuTree.TryGetValue(
                    buttonId,
                    out ToolbarButton? button))
            {
                button.IsVisible = visible;
            }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            double height = double.IsInfinity(availableSize.Height)
                ? 0
                : availableSize.Height;

            double desiredWidth = 0;
            double desiredHeight = 0;

            foreach (Control child in Children)
            {
                if (!child.IsVisible)
                    continue;

                child.Measure(
                    new Size(
                        height > 0
                            ? height
                            : availableSize.Width,
                        height > 0
                            ? height
                            : availableSize.Height));

                double buttonSize = height > 0
                    ? height
                    : Math.Max(
                        child.DesiredSize.Width,
                        child.DesiredSize.Height);

                desiredWidth += buttonSize;
                desiredHeight = Math.Max(
                    desiredHeight,
                    buttonSize);
            }

            return new Size(
                Math.Min(desiredWidth, availableSize.Width),
                Math.Min(desiredHeight, availableSize.Height));

        }
    }
}

