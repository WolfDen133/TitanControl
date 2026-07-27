using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using cToolstrip = TitanControl.Controls.Toolbar.ContextToolstrip;

namespace TitanControl.Controls.Toolbar.Buttons.ContextToolstrip
{
    class RemoveButton : ToolbarButton
    {
        public RemoveButton() : base()
        {
            ID = 6;
            Text = "Remove";
            Description = "Remove a control from the workspace.";
            Toggle = true;

            ButtonImage.Source = ToImage("remove.png");
        }
    }
}
