using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using sToolstrip = TitanControl.Controls.Toolbar.SystemToolstrip;

namespace TitanControl.Controls.Toolbar.Buttons.SystemToolstrip
{
    class SettingsButton : ToolbarButton
    {
        public SettingsButton() : base()
        {
            ID = 1;
            Text = "Settings";
            Description = "Customize TitanControl and modify its settings.";

            Children = [11, 12];

            ButtonSvg.Path = "/Assets/Icons/cog.svg";
        }

        protected override void ClickAction(ButtonAction action)
        {
            Toolstrip.LoadPage(ID);
        }
    }
}
