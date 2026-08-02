using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using cToolstrip = TitanControl.Controls.Toolbar.ContextToolstrip;

namespace TitanControl.Controls.Toolbar.Buttons.ContextToolstrip
{
    class AssignButton : ToolbarButton
    {
        public AssignButton() : base()
        {
            ID = 4;
            Text = "Assign";
            Description = "Assign a handle to a control";

            Toggle = true;

            ButtonSvg.Path = "/Assets/Icons/trigger.svg";
        }
    }
}
