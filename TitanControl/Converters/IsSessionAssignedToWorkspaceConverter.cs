using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using TitanControl.Disk.Model.Session;
using TitanControl.Disk.Model.Workspace;

namespace TitanControl.Converters
{
    public class IsSessionAssignedToWorkspaceConverter : IValueConverter
    {
        public bool Invert { get; set; }

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not SessionModel session)
                return BindingOperations.DoNothing;

            if (parameter is not WorkspaceModel workspace)
                return BindingOperations.DoNothing;

            bool result = workspace.Options.Session == session.ID;

            return Invert ? !result : result;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => BindingOperations.DoNothing;
    }
}
