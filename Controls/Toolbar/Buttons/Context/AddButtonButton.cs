using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using cToolstrip = TitanControl.Controls.Toolbar.ContextToolstrip;

namespace TitanControl.Controls.Toolbar.Buttons.ContextToolstrip
{
    public class AddButtonButton : ToolbarButton
    {
        public AddButtonButton() : base()
        {
            ID = 11;
            Text = "Button";
            Description = "Add a button to your screen.";

            ButtonImage.Source = ToImage("button.png");
        }
    }
}
