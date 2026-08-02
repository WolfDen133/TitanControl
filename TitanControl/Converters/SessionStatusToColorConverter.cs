using Avalonia;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;
using TitanControl.Session;

namespace TitanControl.Converters
{
    internal class SessionStatusToColorConverter : IValueConverter
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

            var resourceKey = state switch
            {
                SessionConnectionState.Available => "AccentColor",
                SessionConnectionState.Inactive => "ForegroundSecondary",
                SessionConnectionState.Connected => "SuccessColor",
                SessionConnectionState.Connecting => "WarningColor",
                SessionConnectionState.Error => "DangerColorD",
                SessionConnectionState.Disconnected => "WarningColorD",
                SessionConnectionState.Unreachable => "DangerColor",
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
                resource is Color brush)
            {
                return brush;
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
