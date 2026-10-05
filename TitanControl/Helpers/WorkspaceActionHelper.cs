using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanControl.ViewModels.Workspace.Controls;

namespace TitanControl.Helpers
{
    public class WorkspaceActionHelper
    {
        public static Size GetMinSpanFromControls(List<IWorkspaceControl> controls)
        {
            int xSpan = 0;
            int ySpan = 0;

            foreach (IWorkspaceControl control in controls)
            {
                Rectangle location = control.Location;

                xSpan = Math.Max(xSpan, location.X + location.Width);
                ySpan = Math.Max(ySpan, location.Y + location.Height);
            }

            return new Size(xSpan, ySpan);
        }
    }
}
