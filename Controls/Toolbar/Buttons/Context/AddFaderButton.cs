using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using cToolstrip = TitanControl.Controls.Toolbar.ContextToolstrip;

namespace TitanControl.Controls.Toolbar.Buttons.ContextToolstrip
{
    public class AddFaderButton : ToolbarButton
    {
        public AddFaderButton() : base()
        {
            ID = 12;
            Text = "Fader";
            Description = "Add a Fader to your screen.";

            ButtonImage.Source = ToImage("fader.png");
        }
    }
}
