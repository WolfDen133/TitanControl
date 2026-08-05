using Avalonia;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;
using TitanControl.Session;

namespace TitanControl.Converters
{
    internal class SessionStatusToBrushConverter : IValueConverter
    {
        public object Convert(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
        {
            if (value is not SessionConnectionState state)
            {
                return new BindingNotification(
                    new ArgumentException(
                        $"Expected {nameof(SessionConnectionState)}."),
                    BindingErrorType.Error);
            }

            double? opacity = null;

            if (parameter is not null)
            {
                if (!double.TryParse(
                        parameter.ToString(),
                        NumberStyles.Float,
                        culture,
                        out var parsedOpacity) ||
                    parsedOpacity is < 0 or > 1)
                {
                    return new BindingNotification(
                        new ArgumentOutOfRangeException(
                            nameof(parameter),
                            "Opacity must be a number between 0 and 1."),
                        BindingErrorType.Error);
                }

                opacity = parsedOpacity;
            }

            var resourceKey = state switch
            {
                SessionConnectionState.Available => "AccentBrush",
                SessionConnectionState.Disabled => "ForegroundSecondaryBrush",
                SessionConnectionState.Enabled => "AccentLBrush",
                SessionConnectionState.Connected => "SuccessBrush",
                SessionConnectionState.Connecting => "WarningBrush",
                SessionConnectionState.Disconnected => "DangerBrush",
                SessionConnectionState.Unreachable => "ForegroundPlaceholderBrush",
                _ => "BorderBrush"
            };

            var application = Application.Current;

            if (application is null)
            {
                return new BindingNotification(
                    new InvalidOperationException("Application.Current is null."),
                    BindingErrorType.Error);
            }

            if (application.TryGetResource(
                    resourceKey,
                    application.ActualThemeVariant,
                    out var resource) &&
                resource is IBrush brush)
            {
                if (opacity is null)
                {
                    return brush;
                }

                if (brush is ISolidColorBrush solidColorBrush)
                {
                    return new SolidColorBrush(
                        solidColorBrush.Color,
                        opacity.Value);
                }

                return new BindingNotification(
                    new InvalidOperationException(
                        $"Brush resource '{resourceKey}' does not support an opacity parameter."),
                    BindingErrorType.Error);
            }

            return new BindingNotification(
                new InvalidOperationException(
                    $"Brush resource '{resourceKey}' was not found."),
                BindingErrorType.Error);
        }

        public object ConvertBack(
            object? value,
            Type targetType,
            object? parameter,
            CultureInfo culture)
        {
            return BindingOperations.DoNothing;
        }
    }
}
