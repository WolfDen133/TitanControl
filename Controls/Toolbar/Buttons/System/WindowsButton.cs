using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using sToolstrip = TitanControl.Controls.Toolbar.SystemToolstrip;

namespace TitanControl.Controls.Toolbar.Buttons.SystemToolstrip
{
    class WindowsButton : ToolbarButton
    {
        public WindowsButton() : base()
        {
            ID = 2;
            Text = "Windows";
            Description = "Open a list of openable windows.";

            Children = [21];

            ButtonImage.Source = ToImage("computer-window.png");
        }

        protected override void ClickAction(ButtonAction action)
        {
            Toolstrip.LoadPage(ID);
        }
    }
}
