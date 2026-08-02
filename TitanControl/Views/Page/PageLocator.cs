using Avalonia.Controls;
using Avalonia.Controls.Templates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TitanControl.Logging;
using TitanControl.ViewModels;

namespace TitanControl.Views.Page
{
    public class PageLocator : IDataTemplate
    {
        public const string LoggingCategory = "ViewLocator";

        private KeyValuePair<string, string> replaceRegex =
            new(@"ViewModels\Pages\(\w+)Model.cs", @"View\Page\Pages\$1Page.cs");

        public Control? Build(object? data)
        {
            if (data is null)
            {
                Log.Error("Page data is null.", LoggingCategory);
                return new TextBlock { Text = "Null page data" };
            }

            var name = Regex.Replace(data.GetType().FullName!, replaceRegex.Key, replaceRegex.Value);

            var viewModelType = Type.GetType(name);

            if (viewModelType is null)
            {
                Log.Error($"Specified view model {name} has no associeated type.", LoggingCategory);
                return new TextBlock { Text = "Invalid view model" };
            }

            return (Control?)Activator.CreateInstance(viewModelType);
        }

        public bool Match (object? data)
        {
            return data is BaseViewModel;
        }
    }
}
