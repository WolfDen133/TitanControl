using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using sToolstrip = TitanControl.Controls.Toolbar.SystemToolstrip;

namespace TitanControl.Controls.Toolbar.Buttons.SystemToolstrip
{
    class DiskButton : ToolbarButton
    {
        public DiskButton() : base()
        {
            ID = 3;
            Text = "Disk";
            Description = "Save, load, or rename a project.";

            Children = [31, 32, 33, 34, 35];

            ButtonImage.Source = ToImage("floppy-disk.png");
        }

        protected override void ClickAction(ButtonAction action)
        {
            Toolstrip.LoadPage(ID);
        }
    }
}
