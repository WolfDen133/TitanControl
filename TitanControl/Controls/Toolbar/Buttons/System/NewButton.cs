using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using sToolstrip = TitanControl.Controls.Toolbar.SystemToolstrip;

namespace TitanControl.Controls.Toolbar.Buttons.SystemToolstrip
{
    class NewButton : ToolbarButton
    {
        public NewButton() : base()
        {
            ID = 35;
            Text = "New";
            Description = "Start new with a blank workspace.";

            ButtonSvg.Path = "/Assets/Icons/new.svg";
        }
    }
}
