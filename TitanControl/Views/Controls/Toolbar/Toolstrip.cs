using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using TitanControl.Logging;
using TitanControl.Views.Controls.Toolbar.Button;
using TitanControl.Views.Controls.Toolbar.Buttons;
using Control = Avalonia.Controls.Control;

namespace TitanControl.Views.Controls.Toolbar
{
    public class Toolstrip : StackPanel
    {
        private const string LogCategory = nameof(Toolstrip);

        private readonly Dictionary<int, ToolbarButton> _buttonsById = new();
        private bool _initialized;
        private int _current = -1;

        public static int MaxPerPage { get; } = 6;

        /// <summary>
        /// Flat collection of every button registered with this Toolstrip.
        /// Buttons may be declared as direct Toolstrip children or nested inside
        /// ToolbarButton.Children in AXAML.
        /// </summary>
        public ObservableCollection<ToolbarButton> MenuTree { get; } = [];
        public List<int> DefaultIndexes = [];

        public bool Exclusive { get; set; } = false;

        public int Current => _current;

        public Toolstrip()
        {
            FlowDirection = Avalonia.Media.FlowDirection.LeftToRight;
            Orientation = Avalonia.Layout.Orientation.Horizontal;
            Margin = new Thickness(4);
        }

        protected override void OnLoaded(RoutedEventArgs e)
        {
            base.OnLoaded(e);

            if (_initialized)
                return;

            _initialized = true;

            // Snapshot the direct AXAML children before we add nested menu children
            // to the StackPanel's visual collection.
            var rootButtons = Children
                .OfType<ToolbarButton>()
                .ToList();

            DefaultIndexes = [.. rootButtons.Where(b => b.ID != -1).Select(b => b.ID)];

            RegisterButtonTree(rootButtons);

            InitializeButtons();
            ShowDefaultPage();
        }

        /// <summary>
        /// Recursively registers buttons declared in ToolbarButton.Children.
        /// Nested buttons are also attached to this StackPanel so visibility/layout
        /// can continue to be controlled by the Toolstrip exactly as before.
        /// </summary>
        private void RegisterButtonTree(IEnumerable<ToolbarButton> buttons)
        {
            foreach (var button in buttons)
            {
                if (_buttonsById.ContainsKey(button.ID))
                {
                    throw new InvalidOperationException(
                        $"A toolbar button with ID {button.ID} has already been registered.");
                }

                _buttonsById.Add(button.ID, button);
                MenuTree.Add(button);

                // The Toolstrip owns the toolbar button's UI lifetime.
                button.Toolstrip = this;

                // Snapshot because descendants will be attached to Children below.
                var childButtons = button.Children.ToList();

                RegisterButtonTree(childButtons);

                foreach (var child in childButtons)
                {
                    if (!Children.Contains(child))
                        Children.Add(child);
                }
            }
        }

        private void InitializeButtons()
        {
            foreach (var button in MenuTree)
            {
                button.OnClick += OnButtonClick;
            }
        }

        private async void OnButtonClick(
            object? sender,
            ButtonAction action)
        {
            if (sender is not ToolbarButton selectedButton)
                return;

            if (selectedButton.Children.Count > 0)
            {
                await ShowPageAfter(selectedButton, selectedButton.Children.Count > 0);
            }

            if (selectedButton.ID == -1)
            {
                await ShowPageAfter(null, false);
                return;
            }

            if (!Exclusive) return;

            foreach (var button in MenuTree)
            {
                if (button.ID != selectedButton.ID)
                    button.ReleaseToggle(true);
            }
        }

        protected virtual void ShowDefaultPage()
        {
            ShowPage(null, includeBackButton: false);
        }

        public void LoadPage(int id)
        {
            if (id == 0 || id == -1)
            {
                ShowDefaultPage();
                return;
            }

            if (!_buttonsById.TryGetValue(id, out var pageButton))
            {
                Log.Warning(
                    $"Unable to load toolbar page {id}: the page does not exist.",
                    LogCategory);

                return;
            }

            if (pageButton.Children?.Count == 0)
            {
                Log.Warning(
                    $"Unable to load toolbar page {id}: the page has no child buttons.",
                    LogCategory);

                return;
            }

            ShowPage(pageButton, includeBackButton: true);
        }

        private async Task ShowPageAfter(ToolbarButton? page, bool includeBackButton)
        {
            await Task.Delay(50);

            ShowPage(page, includeBackButton);
        }

        private void ShowPage(
            ToolbarButton? page,
            bool includeBackButton)
        {
            foreach (var button in MenuTree)
                button.IsVisible = false;

            if (includeBackButton &&
                _buttonsById.TryGetValue(-1, out var backButton))
            {
                backButton.IsVisible = true;
            }

            if (page is null)
            {
                ShowDefaultButtons();
                _current = -1;

                InvalidateMeasure();
                InvalidateArrange();
                return;
            }

            // Children are now the actual ToolbarButton instances.
            foreach (var child in page.Children)
                child.IsVisible = true;

            _current = page.ID;

            InvalidateMeasure();
            InvalidateArrange();
        }

        protected virtual void ShowDefaultButtons()
        {
            foreach (var index in DefaultIndexes)
                SetButtonVisible(index);
        }

        protected void SetButtonVisible(int buttonId, bool visible = true)
        {
            if (_buttonsById.TryGetValue(buttonId, out var button))
                button.IsVisible = visible;
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

