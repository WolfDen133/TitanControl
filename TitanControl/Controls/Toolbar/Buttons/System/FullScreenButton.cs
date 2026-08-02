using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using sToolstrip = TitanControl.Controls.Toolbar.SystemToolstrip;

namespace TitanControl.Controls.Toolbar.Buttons.SystemToolstrip
{
    class FullScreenButton : ToolbarButton
    {
        public FullScreenButton() : base()
        {
            ID = 41;
            Text = "Full Screen";
            Description = "Put TitanControl into fullscreen.";

            ButtonSvg.Path = "/Assets/Icons/expand.svg";
        }
    }
}
