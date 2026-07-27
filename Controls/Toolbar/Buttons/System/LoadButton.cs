using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using sToolstrip = TitanControl.Controls.Toolbar.SystemToolstrip;

namespace TitanControl.Controls.Toolbar.Buttons.SystemToolstrip
{
    class LoadButton : ToolbarButton
    {
        public LoadButton() : base()
        {
            ID = 33;
            Text = "Load";
            Description = "Load an existing workspace file.";

            ButtonImage.Source = ToImage("upload.png");
        }
    }
}
