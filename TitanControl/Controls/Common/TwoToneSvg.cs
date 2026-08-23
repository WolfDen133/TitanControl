using Avalonia;
using Avalonia.Media;
using System;


namespace TitanControl.Controls.Common
{
    public class TwoToneSvg : Avalonia.Svg.Skia.Svg
    {
        public static readonly StyledProperty<Color> PrimaryColorProperty =
            AvaloniaProperty.Register<TwoToneSvg, Color>(
                nameof(PrimaryColor),
                Colors.Black);

        public static readonly StyledProperty<Color> SecondaryColorProperty =
            AvaloniaProperty.Register<TwoToneSvg, Color>(
                nameof(SecondaryColor),
                Colors.Gray);

        public Color PrimaryColor
        {
            get => GetValue(PrimaryColorProperty);
            set => SetValue(PrimaryColorProperty, value);
        }

        public Color SecondaryColor
        {
            get => GetValue(SecondaryColorProperty);
            set => SetValue(SecondaryColorProperty, value);
        }

        public TwoToneSvg(Uri baseUri) : base(baseUri)
        {
            UpdateCss();
        }

        public TwoToneSvg(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            UpdateCss();
        }

        static TwoToneSvg()
        {
            PrimaryColorProperty.Changed.AddClassHandler<TwoToneSvg>(
                static (control, _) => control.UpdateCss());

            SecondaryColorProperty.Changed.AddClassHandler<TwoToneSvg>(
                static (control, _) => control.UpdateCss());
        }

        private void UpdateCss()
        {
            SetCss(this, $$"""
            .primary {
                fill: {{ToSvgColor(PrimaryColor)}};
            }

            .primary-stroke {
                stroke: {{ToSvgColor(PrimaryColor)}};
            }

            .secondary {
                fill: {{ToSvgColor(SecondaryColor)}};
            }

            .secondary-stroke {
                stroke: {{ToSvgColor(SecondaryColor)}};
            }
            """);
        }

        private static string ToSvgColor(Color color)
        {
            // Use CSS-compatible #RRGGBB or #RRGGBBAA rather than Avalonia's
            // usual #AARRGGBB representation.
            return color.A == byte.MaxValue
                ? $"#{color.R:X2}{color.G:X2}{color.B:X2}"
                : $"rgba({color.R}, {color.G}, {color.B}, {color.A / 255d:0.###})";
        }
    }
}
