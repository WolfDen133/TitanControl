using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Svg.Skia;
using Svg.Model;

namespace TitanControl.Utils
{

    public static class ThemedSvg
    {
        public static SvgSource Load(
            Control resourceOwner,
            string svgUri,
            object primaryBrushKey,
            object secondaryBrushKey)
        {
            var primary = ResolveColor(resourceOwner, primaryBrushKey);
            var secondary = ResolveColor(resourceOwner, secondaryBrushKey);

            var source = SvgSource.Load(
                svgUri,
                new Uri("avares://TitanControl/"));

            var css = $$"""
            .primary {
                fill: {{ToCss(primary)}} !important;
            }

            .secondary {
                fill: {{ToCss(secondary)}} !important;
            }
            """;

            source.ReLoad(new SvgParameters(
                entities: null,
                css: css));

            return source;
        }

        private static Color ResolveColor(
            Control owner,
            object resourceKey)
        {
            // First try the requested theme-specific lookup from the owner.
            if (!owner.TryFindResource(
                    resourceKey,
                    owner.ActualThemeVariant,
                    out var value))
            {
                // If that fails, try a non-theme lookup on the owner (broader search).
                if (!owner.TryFindResource(resourceKey, out value))
                {
                    // As a final fallback, try application-level resources if available.
                    var app = Application.Current;
                    if (app == null
                        || (!app.TryFindResource(resourceKey, owner.ActualThemeVariant, out value)
                            && !app.TryFindResource(resourceKey, out value)))
                    {
                        throw new InvalidOperationException(
                            $"Resource '{resourceKey}' was not found.");
                    }
                }
            }

            return value switch
            {
                ISolidColorBrush brush => brush.Color,
                Color color => color,
                _ => throw new InvalidOperationException(
                    $"Resource '{resourceKey}' must be a solid brush or Color.")
            };
        }

        private static string ToCss(Color color)
        {
            var alpha = color.A / 255d;
            return FormattableString.Invariant(
                $"rgba({color.R},{color.G},{color.B},{alpha:0.###})");
        }
    }
}
