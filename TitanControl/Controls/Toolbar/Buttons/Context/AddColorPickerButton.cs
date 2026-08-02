using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using cToolstrip = TitanControl.Controls.Toolbar.ContextToolstrip;

namespace TitanControl.Controls.Toolbar.Buttons.ContextToolstrip
{
    public class AddColorPickerButton : ToolbarButton
    {
        public AddColorPickerButton() : base()
        {
            ID = 13;
            Text = "Color Picker";
            Description = "Add a Color Picker to your screen.";

            ButtonImage.Source = ToImage("colorpicker.png");
        }
    }
}
