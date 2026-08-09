using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Svg.Skia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TitanControl.Helper
{
    public class ResourceHelper
    {
        private static App? _app;

        public ResourceHelper(App app)
        {
            _app = app;
        }

        public static SvgSource GetSvgSource(string key)
        {
            if (_app is null)
                throw new InvalidOperationException("ResourceHelper is not initialized. Ensure that App.ResourceHelper is set in App.Initialize.");
            

            // Search the full logical tree (most common usage)
            if (_app.TryFindResource(key, out var found))
            {
                if (found is SvgSource)
                    return (SvgSource)found;
                
            }

            return new SvgSource(new Uri("avares://TitnaControl/Assets/Icons/icon.svg"));
        }

        public static IBrush GetThemeBrush(string key)
        {
            if (_app is null)
                throw new InvalidOperationException("ResourceHelper is not initialized. Ensure that App.ResourceHelper is set in App.Initialize.");
            

            // Search the full logical tree (most common usage)
            if (_app.TryFindResource(key, _app.ActualThemeVariant, out var found))
            {
                if (found is IBrush)
                    return (IBrush)found;
            }
             
            return Brushes.Red;
        }
    }
}
