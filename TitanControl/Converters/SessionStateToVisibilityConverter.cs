using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Globalization;
using TitanControl.Services.Session;

namespace TitanControl.Converters
{
    public class SessionStateToVisibilityConverter : IValueConverter
    {

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is SessionConnectionState state && parameter is string type)
            {
                switch (type.ToLower())
                {
                    case "connect": return state is SessionConnectionState.Enabled or SessionConnectionState.Unreachable;
                    case "disconnect": return state is SessionConnectionState.Connected or SessionConnectionState.Connecting;
                    case "enable": return state is SessionConnectionState.Disabled;
                    case "disable": return state is SessionConnectionState.Enabled or SessionConnectionState.Unreachable;
                }
            }

            return BindingOperations.DoNothing;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
