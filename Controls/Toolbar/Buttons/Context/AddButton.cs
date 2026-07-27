using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using TitanControl.Utils;
using cToolstrip = TitanControl.Controls.Toolbar.ContextToolstrip;

namespace TitanControl.Controls.Toolbar.Buttons.ContextToolstrip
{
    class AddButton : ToolbarButton
    {
        public AddButton() : base()
        {
            ID = 1;
            Text = "Add";
            Description = "Add a new control to your screen.";

            Children = [11, 12, 13];

            // ButtonImage.Source = ("add.png");
            ButtonSvg.Path = "/Assets/Icons/add.svg";
        }


        protected override void ClickAction(ButtonAction action)
        {
            Toolstrip.LoadPage(ID);
        }
    }
}
