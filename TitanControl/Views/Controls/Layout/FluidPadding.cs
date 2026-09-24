using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;

namespace TitanControl.Views.Controls.Layout
{
    public static class FluidPadding
    {
        private const double ReferenceWidth = 1920;
        private const double ReferenceHeight = 1080;

        public static readonly AttachedProperty<Thickness> ReferencePaddingProperty =
            AvaloniaProperty.RegisterAttached<Control, Thickness>(
                "ReferencePadding",
                typeof(FluidPadding),
                default);

        private sealed class Subscription
        {
            public required Window Window { get; init; }
            public required EventHandler<SizeChangedEventArgs> Handler { get; init; }
        }

        private static readonly Dictionary<Control, Subscription> Subscriptions = new();

        static FluidPadding()
        {
            ReferencePaddingProperty.Changed.AddClassHandler<Control>(
                OnReferencePaddingChanged);
        }

        public static Thickness GetReferencePadding(Control control) =>
            control.GetValue(ReferencePaddingProperty);

        public static void SetReferencePadding(
            Control control,
            Thickness value) =>
            control.SetValue(ReferencePaddingProperty, value);

        private static void OnReferencePaddingChanged(
            Control control,
            AvaloniaPropertyChangedEventArgs e)
        {
            // Subscribe only once, even if the property changes repeatedly.
            control.AttachedToVisualTree -= OnAttached;
            control.DetachedFromVisualTree -= OnDetached;

            control.AttachedToVisualTree += OnAttached;
            control.DetachedFromVisualTree += OnDetached;

            Attach(control);
        }

        private static void OnAttached(
            object? sender,
            VisualTreeAttachmentEventArgs e)
        {
            if (sender is Control control)
                Attach(control);
        }

        private static void OnDetached(
            object? sender,
            VisualTreeAttachmentEventArgs e)
        {
            if (sender is Control control)
                Detach(control);
        }

        private static void Attach(Control control)
        {
            Detach(control);

            if (TopLevel.GetTopLevel(control) is not Window window)
                return;

            EventHandler<SizeChangedEventArgs> handler =
                (_, _) => UpdatePadding(control, window);

            Subscriptions[control] = new Subscription
            {
                Window = window,
                Handler = handler
            };

            window.SizeChanged += handler;

            UpdatePadding(control, window);
        }

        private static void Detach(Control control)
        {
            if (!Subscriptions.Remove(control, out var subscription))
                return;

            subscription.Window.SizeChanged -= subscription.Handler;
        }

        private static void UpdatePadding(Control control, Window window)
        {
            if (control is not Border border)
                return;

            Thickness reference = GetReferencePadding(control);

            double width = window.ClientSize.Width;

            if (width is < 0 or > ReferenceWidth)
                return;

            // One uniform scale, preserving the proportions of the padding.
            double scale = width / ReferenceWidth;

            border.Padding = new Thickness(
                reference.Left * scale,
                reference.Top * scale,
                reference.Right * scale,
                reference.Bottom * scale);
        }
    }
}
