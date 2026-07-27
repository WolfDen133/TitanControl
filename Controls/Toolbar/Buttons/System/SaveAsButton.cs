using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using sToolstrip = TitanControl.Controls.Toolbar.SystemToolstrip;

namespace TitanControl.Controls.Toolbar.Buttons.SystemToolstrip
{
    class SaveAsButton : ToolbarButton
    {
        public SaveAsButton() : base()
        {
            ID = 32;
            Text = "Save As";
            Description = "Save your open workspace to a new location.";

            ButtonImage.Source = ToImage("save-as.png");
        }
    }
}
