using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using cToolstrip = TitanControl.Controls.Toolbar.ContextToolstrip;

namespace TitanControl.Controls.Toolbar.Buttons.ContextToolstrip
{
    class CopyButton : ToolbarButton
    {
        public CopyButton() : base()
        {
            ID = 2;
            Text = "Copy";
            Description = "Copy one control to another place.";
            Toggle = true;

            ButtonImage.Source = ToImage("copy.png");
        }


        protected override void ClickAction(ButtonAction action)
        {


        }
    }
}
