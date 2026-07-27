using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using sToolstrip = TitanControl.Controls.Toolbar.SystemToolstrip;

namespace TitanControl.Controls.Toolbar.Buttons.SystemToolstrip
{
    class CircleBuilderButton : ToolbarButton
    {
        public CircleBuilderButton() : base()
        {
            ID = 21;
            Text = "Circle Builder";
            Description = "Open the circle builder for SpotViz and Capture Visualiser.";

            ButtonImage.Source = ToImage("circle.png");
        }

        protected override void ClickAction(ButtonAction action)
        {
            
        }
    }
}
