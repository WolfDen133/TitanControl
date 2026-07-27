using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using cToolstrip = TitanControl.Controls.Toolbar.ContextToolstrip;

namespace TitanControl.Controls.Toolbar.Buttons.ContextToolstrip
{
    class PropertiesButton : ToolbarButton
    {
        public PropertiesButton() : base()
        {
            ID = 5;
            Text = "Properties";
            Description = "Change the properties of a selected control";
            Toggle = true;

            ButtonImage.Source = ToImage("properties.png");
        }
    }
}
