using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using sToolstrip = TitanControl.Controls.Toolbar.SystemToolstrip;

namespace TitanControl.Controls.Toolbar.Buttons.SystemToolstrip
{
    class SaveButton : ToolbarButton
    {
        public SaveButton() : base()
        {
            ID = 31;
            Text = "Save";
            Description = "Save your current workspace to disk.";

            ButtonImage.Source = ToImage("floppy-disk.png");
        }
    }
}
