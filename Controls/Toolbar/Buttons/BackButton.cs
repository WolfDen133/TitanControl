using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanControl.Controls.Toolbar;

namespace TitanControl.Controls.Toolbar.Buttons
{
    class BackButton : ToolbarButton
    {
        public BackButton() : base()
        {
            ID = -1;
            Text = "Back";
            Description = "Return to the previous menu.";

            ButtonImage.Source = ToImage("arrow.png");
        }

        protected override void ClickAction(ButtonAction action)
        {
            Toolstrip.LoadPage(0);
        }
    }
}
