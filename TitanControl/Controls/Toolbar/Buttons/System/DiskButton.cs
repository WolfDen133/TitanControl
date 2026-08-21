using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TitanControl.Controls.Toolbar.Buttons.SystemToolstrip
{
    class DiskButton : ToolbarButton
    {
        public DiskButton() : base()
        {
            ID = 2;
            Text = "Disk";
            Description = "Save, load, or rename a project.";

            Children = [21, 22, 23, 24, 25];

            ButtonSvg.Path = "/Assets/Icons/disk.svg";
        }

        protected override void ClickAction(ButtonAction action)
        {
            Toolstrip.LoadPage(ID);
        }
    }
}
