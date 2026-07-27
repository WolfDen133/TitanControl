using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cToolstrip = TitanControl.Controls.Toolbar.ContextToolstrip;

namespace TitanControl.Controls.Toolbar.Buttons.ContextToolstrip
{
    class MoveButton : ToolbarButton
    {
        public MoveButton() : base()
        {
            ID = 3;
            Text = "Move";
            Description = "Move a control from one place to another";
            Toggle = true;

            ButtonImage.Source = ToImage("move.png");
        }
    }
}
