using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using cToolstrip = TitanControl.Controls.Toolbar.ContextToolstrip;

namespace TitanControl.Controls.Toolbar.Buttons.ContextToolstrip
{
    class OptionsButton : ToolbarButton
    {
        public OptionsButton() : base()
        {
            ID = 5;
            Text = "Options";
            Description = "Change the properties of a selected control";
            Toggle = true;

            ButtonSvg.Path = "/Assets/Icons/options.svg";
        }
    }
}
