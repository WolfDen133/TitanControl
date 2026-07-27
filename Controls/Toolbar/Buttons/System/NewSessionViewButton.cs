using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using sToolstrip = TitanControl.Controls.Toolbar.SystemToolstrip;

namespace TitanControl.Controls.Toolbar.Buttons.SystemToolstrip
{
    class NewSessionViewButton : ToolbarButton
    {
        public NewSessionViewButton() : base()
        {
            ID = -1;
            Text = "Windows";
            Description = "Open the different windows TitanControl has to offer.";

            ButtonImage.Source = ToImage("computer-window.png");
        }

        protected override void ClickAction(ButtonAction action)
        {
            Toolstrip.LoadPage(ID);
        }
    }
}
