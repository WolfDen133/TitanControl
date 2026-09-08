using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using TitanControl.Events.Control;
using TitanControl.Logging;
using TitanControl.Views.Controls.Toolbar.Button;
using TitanControl.Views.Controls.Toolbar.Buttons;
using Control = Avalonia.Controls.Control;

namespace TitanControl.Views.Controls.Toolbar
{
    public class Toolstrip : StackPanel
    {
        private const string LogCategory = nameof(Toolstrip);

        public static int MaxPerPage => 6;

        private readonly Dictionary<ButtonId, ToolbarButton> _buttonsById = new();
        private readonly Dictionary<ButtonId, Control> _containersById = new();
        private ButtonId _current = ButtonId.None;


        public ObservableCollection<ToolbarButton> MenuTree { get; } = [];
        public List<ButtonId> DefaultIds = [];

        public bool Exclusive { get; set; } = false;

        public ButtonId Current => _current;

        public static readonly RoutedEvent<ToolButtonPressedEventArgs>
            ToolButtonPressedEvent =
                RoutedEvent.Register<Toolstrip, ToolButtonPressedEventArgs>(
                    nameof(ToolButtonPressed),
                    RoutingStrategies.Bubble);


        public event EventHandler<ToolButtonPressedEventArgs> ToolButtonPressed
        {
            add => AddHandler(ToolButtonPressedEvent, value);
            remove => RemoveHandler(ToolButtonPressedEvent, value);
        }

        public Toolstrip()
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal;
            Margin = new Thickness(4);
        }

        protected override void OnLoaded(RoutedEventArgs e)
        {
            base.OnLoaded(e);

            RebuildButtonIndex();
        }

        protected override void OnUnloaded(RoutedEventArgs e)
        {
            UnregisterButtons();

            base.OnUnloaded(e);
        }

        private void RebuildButtonIndex()
        {
            UnregisterButtons();

            var generatedButtons = Children
                .OfType<Control>()
                .Select(container => new
                {
                    Container = container,
                    Button = ResolveToolbarButton(container)
                })
                .Where(entry => entry.Button is not null)
                .Select(entry => new
                {
                    entry.Container,
                    Button = entry.Button!
                })
                .ToList();

            foreach (var entry in generatedButtons)
            {
                var button = entry.Button;

                if (!_buttonsById.TryAdd(button.Id, button))
                {
                    throw new InvalidOperationException(
                        $"A toolbar button with ID {button.Id} " +
                        "has already been registered.");
                }

                _containersById.Add(button.Id, entry.Container);
                MenuTree.Add(button);

                button.Toolstrip = this;
                button.OnClick += OnButtonClick;
            }

            // Determine which generated buttons are children of other buttons.
            var childIds = generatedButtons
                .SelectMany(entry => entry.Button.Children)
                .ToHashSet();

            DefaultIds =
            [
                .. generatedButtons
                .Select(entry => entry.Button)
                .Where(button =>
                    ((int)button.Id) <= 10
                    && button.Id != ButtonId.Back)
                .Select(b => b.Id)
            ];

            ValidateChildButtons();

            ShowDefaultPage();
        }

        private void UnregisterButtons()
        {
            foreach (var button in MenuTree)
            {
                button.OnClick -= OnButtonClick;
                button.Toolstrip = null;
            }

            MenuTree.Clear();
            _buttonsById.Clear();
            _containersById.Clear();
            DefaultIds.Clear();
        }

        private void ValidateChildButtons()
        {
            foreach (var parent in MenuTree)
            {
                foreach (var child in parent.Children)
                {
                    if (_buttonsById.ContainsKey(child))
                        continue;

                    throw new InvalidOperationException(
                        $"Toolbar button {parent.Id} references child " +
                        $"{child}, but {child} is not present in the " +
                        "ItemsControl.ItemsSource.");
                }
            }
        }

        private async void OnButtonClick(
            object? sender,
            ButtonAction action)
        {
            if (sender is not ToolbarButton selectedButton)
                return;

            if (Exclusive && action != ButtonAction.ToggleUp)
            {
                foreach (var button in MenuTree)
                {
                    if (button.Id != selectedButton.Id)
                        button.ReleaseToggle();
                }
            }

            if (selectedButton.Children.Count > 0)
            {
                await ShowPageAfter(selectedButton, selectedButton.Children.Count > 0);
            }

            if (selectedButton.Id == ButtonId.Back)
            {
                await ShowPageAfter(null, false);
                return;
            }

            RaiseEvent(new ToolButtonPressedEventArgs(ToolButtonPressedEvent) { ButtonAction = action, ButtonId = selectedButton.Id });
        }

        protected virtual void ShowDefaultPage()
        {
            ShowPage(null, includeBackButton: false);
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
            foreach (var container in _containersById.Values)
                container.IsVisible = false;

            if (includeBackButton)
                SetButtonVisible(ButtonId.Back);

            if (page is null)
            {
                ShowDefaultButtons();
                _current = ButtonId.None;

                InvalidateMeasure();
                InvalidateArrange();
                return;
            }

            foreach (var child in page.Children)
                SetButtonVisible(child);

            _current = page.Id;

            InvalidateMeasure();
            InvalidateArrange();
        }

        protected virtual void ShowDefaultButtons()
        {
            foreach (var index in DefaultIds)
                SetButtonVisible(index);
        }

        protected void SetButtonVisible(
            ButtonId buttonId,
            bool visible = true)
        {
            if (_containersById.TryGetValue(buttonId, out var container))
                container.IsVisible = visible;
        }

        private static ToolbarButton? ResolveToolbarButton(Control container)
        {
            // Supports the old direct-child approach.
            if (container is ToolbarButton directButton)
                return directButton;

            if (container is ContentPresenter presenter)
            {
                // ItemsSource contains actual ToolbarButton instances.
                if (presenter.Content is ToolbarButton contentButton)
                    return contentButton;

                // ItemsSource contains view models and a DataTemplate creates
                // the ToolbarButton.
                return presenter
                    .GetVisualDescendants()
                    .OfType<ToolbarButton>()
                    .FirstOrDefault();
            }

            return container
                .GetVisualDescendants()
                .OfType<ToolbarButton>()
                .FirstOrDefault();
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

